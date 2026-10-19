namespace PTickets.Modules.Notices.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.IssueNotice;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class NoticeFacadeAndEventHandlerTests
{
    private static NoticesDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<NoticesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NoticesDbContext(options);
    }

    [Fact]
    public async Task NoticeIssuedEventHandler_WhenEventHandled_SavesNoticeAndQueuesOutboxMessageAtomically()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var handler = new NoticeIssuedEventHandler(db);

        var noticeId = NoticeId.New();
        var inspectionId = InspectionId.New();
        var reg = new RegistrationNumber("KR99999");
        var issuedAt = new DateTime(2026, 10, 15, 14, 30, 0, DateTimeKind.Utc);

        var evt = new NoticeIssuedEvent(noticeId, inspectionId, reg, 150m, 25m, issuedAt);

        // Act
        await handler.Handle(evt, CancellationToken.None);

        // Assert
        var savedNotice = await db.Notices.SingleAsync();
        savedNotice.Id.Should().Be(noticeId);
        savedNotice.TotalAmount.Should().Be(175m);
        savedNotice.Status.Should().Be(NoticeStatus.Issued);

        var outboxMessage = await db.OutboxMessages.SingleAsync();
        outboxMessage.NoticeId.Should().Be(noticeId.Value);
        outboxMessage.ProcessedAt.Should().BeNull();
        outboxMessage.RetryCount.Should().Be(0);
        outboxMessage.PayloadJson.Should().Contain("KR99999");
    }

    [Fact]
    public async Task WasNoticeIssuedForDateAsync_WhenActiveNoticeExistsOnSameDay_ReturnsTrue()
    {
        await using var db = CreateInMemoryDbContext();
        var reg = new RegistrationNumber("WA11111");
        var issuedAt = new DateTime(2026, 10, 15, 10, 15, 0, DateTimeKind.Utc);

        db.Notices.Add(Notice.Create(NoticeId.New(), InspectionId.New(), reg, 150m, 0m, issuedAt));
        await db.SaveChangesAsync();

        var facade = new NoticeFacade(db);

        var result = await facade.WasNoticeIssuedForDateAsync(reg, new DateTime(2026, 10, 15), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WasNoticeIssuedForDateAsync_WhenNoticeWasIssuedYesterdayJustBeforeMidnight_ReturnsFalseForToday()
    {
        await using var db = CreateInMemoryDbContext();
        var reg = new RegistrationNumber("WA11111");
        var yesterdayLate = new DateTime(2026, 10, 14, 23, 59, 59, DateTimeKind.Utc);

        db.Notices.Add(Notice.Create(NoticeId.New(), InspectionId.New(), reg, 150m, 0m, yesterdayLate));
        await db.SaveChangesAsync();

        var facade = new NoticeFacade(db);

        var resultToday = await facade.WasNoticeIssuedForDateAsync(reg, new DateTime(2026, 10, 15, 0, 1, 0, DateTimeKind.Utc), CancellationToken.None);

        resultToday.Should().BeFalse();
    }

    [Fact]
    public async Task WasNoticeIssuedForDateAsync_WhenTodayNoticeWasCancelled_ReturnsFalseAllowingNewInspection()
    {
        await using var db = CreateInMemoryDbContext();
        var reg = new RegistrationNumber("WA11111");
        var issuedAt = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

        var cancelledNotice = Notice.Create(NoticeId.New(), InspectionId.New(), reg, 150m, 0m, issuedAt);
        cancelledNotice.Cancel();

        db.Notices.Add(cancelledNotice);
        await db.SaveChangesAsync();

        var facade = new NoticeFacade(db);

        var result = await facade.WasNoticeIssuedForDateAsync(reg, new DateTime(2026, 10, 15), CancellationToken.None);

        result.Should().BeFalse();
    }
}

