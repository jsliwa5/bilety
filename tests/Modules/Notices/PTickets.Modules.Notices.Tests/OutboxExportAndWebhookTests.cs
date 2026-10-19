namespace PTickets.Modules.Notices.Tests;

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.Common.Exceptions;
using PTickets.Modules.Notices.ExternalSync;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class OutboxExportAndWebhookTests
{
    private static NoticesDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<NoticesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NoticesDbContext(options);
    }

    private static IConfiguration CreateConfiguration(string apiKey = "secret-webhook-key-123")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExternalNoticesSystem:ApiKey"] = apiKey,
                ["ExternalNoticesSystem:BaseUrl"] = "http://localhost:5095"
            })
            .Build();
    }

    [Fact]
    public async Task ExportUnsyncedNoticesAsync_WhenExternalSystemReturns200Ok_MarksOutboxMessagesAsProcessed()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();

        var item = new ExternalNoticeItemDto(Guid.NewGuid(), Guid.NewGuid(), "WA12345", 150m, 0m, 150m, "Issued", DateTime.UtcNow);
        var outboxMsg = NoticeOutboxMessage.Create(item.NoticeId, "NoticeCreatedForDailyExport", JsonSerializer.Serialize(item), DateTime.UtcNow);

        db.OutboxMessages.Add(outboxMsg);
        await db.SaveChangesAsync();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost:5095") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient("ExternalNoticesClient")).Returns(httpClient);

        var service = new DailyNoticesExportService(
            db,
            httpClientFactoryMock.Object,
            CreateConfiguration(),
            NullLogger<DailyNoticesExportService>.Instance);

        // Act
        var exportedCount = await service.ExportUnsyncedNoticesAsync(CancellationToken.None);
        var secondRunCount = await service.ExportUnsyncedNoticesAsync(CancellationToken.None);

        // Assert
        exportedCount.Should().Be(1);
        secondRunCount.Should().Be(0); // Idempotentność - za drugim razem nie ma już nic do wysłania

        var updatedMsg = await db.OutboxMessages.SingleAsync();
        updatedMsg.ProcessedAt.Should().NotBeNull();
        updatedMsg.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task ExportUnsyncedNoticesAsync_WhenExternalSystemReturns503_IncrementsRetryCountAndKeepsProcessedAtNull()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();

        var item = new ExternalNoticeItemDto(Guid.NewGuid(), Guid.NewGuid(), "WA12345", 150m, 0m, 150m, "Issued", DateTime.UtcNow);
        var outboxMsg = NoticeOutboxMessage.Create(item.NoticeId, "NoticeCreatedForDailyExport", JsonSerializer.Serialize(item), DateTime.UtcNow);

        db.OutboxMessages.Add(outboxMsg);
        await db.SaveChangesAsync();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost:5095") };
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient("ExternalNoticesClient")).Returns(httpClient);

        var service = new DailyNoticesExportService(
            db,
            httpClientFactoryMock.Object,
            CreateConfiguration(),
            NullLogger<DailyNoticesExportService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.ExportUnsyncedNoticesAsync(CancellationToken.None));

        var failedMsg = await db.OutboxMessages.SingleAsync();
        failedMsg.ProcessedAt.Should().BeNull();
        failedMsg.RetryCount.Should().Be(1);
        failedMsg.LastError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenApiKeyIsInvalid_ReturnsUnauthorizedAndDoesNotChangeNoticeStatus()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var notice = Notice.Create(NoticeId.New(), InspectionId.New(), new RegistrationNumber("WA12345"), 150m, 0m, DateTime.UtcNow);
        db.Notices.Add(notice);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Api-Key"] = "wrong-key";

        var payload = new NoticeStatusWebhookPayload(Guid.NewGuid(), notice.Id.Value, "PAID", null, DateTime.UtcNow);

        // Act
        var result = await ExternalSyncEndpoints.HandleWebhookAsync(
            httpContext,
            payload,
            db,
            CreateConfiguration("secret-webhook-key-123"),
            NullLoggerFactory.Instance,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedHttpResult>();

        var unchangedNotice = await db.Notices.SingleAsync();
        unchangedNotice.Status.Should().Be(NoticeStatus.Issued);
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenValidAppealAndThenPaidFlow_UpdatesNoticeStatusInDatabase()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var notice = Notice.Create(NoticeId.New(), InspectionId.New(), new RegistrationNumber("WA12345"), 150m, 0m, DateTime.UtcNow);
        db.Notices.Add(notice);
        await db.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Api-Key"] = "secret-webhook-key-123";
        var config = CreateConfiguration("secret-webhook-key-123");

        // Act 1: Kierowca składa odwołanie
        await ExternalSyncEndpoints.HandleWebhookAsync(
            httpContext,
            new NoticeStatusWebhookPayload(Guid.NewGuid(), notice.Id.Value, "APPEALED", "Zepsuty parkomat", DateTime.UtcNow),
            db, config, NullLoggerFactory.Instance, CancellationToken.None);

        (await db.Notices.SingleAsync()).Status.Should().Be(NoticeStatus.Appealed);

        // Act 2: Odwołanie odrzucone -> wraca do Issued
        await ExternalSyncEndpoints.HandleWebhookAsync(
            httpContext,
            new NoticeStatusWebhookPayload(Guid.NewGuid(), notice.Id.Value, "APPEAL_REJECTED", "Parkomat działał", DateTime.UtcNow),
            db, config, NullLoggerFactory.Instance, CancellationToken.None);

        (await db.Notices.SingleAsync()).Status.Should().Be(NoticeStatus.Issued);

        // Act 3: Przekazano do windykacji
        await ExternalSyncEndpoints.HandleWebhookAsync(
            httpContext,
            new NoticeStatusWebhookPayload(Guid.NewGuid(), notice.Id.Value, "SENT_TO_DEBT_COLLECTION", null, DateTime.UtcNow),
            db, config, NullLoggerFactory.Instance, CancellationToken.None);

        (await db.Notices.SingleAsync()).Status.Should().Be(NoticeStatus.SentToDebtCollection);

        // Act 4: Opłacono
        await ExternalSyncEndpoints.HandleWebhookAsync(
            httpContext,
            new NoticeStatusWebhookPayload(Guid.NewGuid(), notice.Id.Value, "PAID", null, DateTime.UtcNow),
            db, config, NullLoggerFactory.Instance, CancellationToken.None);

        (await db.Notices.SingleAsync()).Status.Should().Be(NoticeStatus.Paid);
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenNoticeDoesNotExist_ThrowsNoticeNotFoundException()
    {
        await using var db = CreateInMemoryDbContext();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Api-Key"] = "secret-webhook-key-123";

        var payload = new NoticeStatusWebhookPayload(Guid.NewGuid(), Guid.NewGuid(), "PAID", null, DateTime.UtcNow);

        await Assert.ThrowsAsync<NoticeNotFoundException>(() =>
            ExternalSyncEndpoints.HandleWebhookAsync(
                httpContext,
                payload,
                db,
                CreateConfiguration("secret-webhook-key-123"),
                NullLoggerFactory.Instance,
                CancellationToken.None));
    }
}

