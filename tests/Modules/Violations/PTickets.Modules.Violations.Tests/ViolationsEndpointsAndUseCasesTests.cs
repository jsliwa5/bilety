namespace PTickets.Modules.Violations.Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations.Common.Exceptions;
using PTickets.Modules.Violations.CreateSurchargeTier;
using PTickets.Modules.Violations.CreateViolationType;
using PTickets.Modules.Violations.SetPenaltyAmount;
using PTickets.Shared.Abstractions;
using Xunit;

public class ViolationsEndpointsAndUseCasesTests
{
    private static ViolationsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ViolationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ViolationsDbContext(options);
    }

    [Fact]
    public async Task SetPenaltyAmount_WhenEffectiveFromIsBeforeToday_ThrowsInvalidPenaltyEffectiveDateException()
    {
        await using var db = CreateInMemoryDbContext();
        var violationType = ViolationType.Create("Postój na kopercie");
        db.ViolationTypes.Add(violationType);
        await db.SaveChangesAsync();

        var today = new DateTime(2026, 10, 19, 10, 0, 0, DateTimeKind.Utc);
        var dateTimeMock = new Mock<IDateTimeProvider>();
        dateTimeMock.Setup(d => d.UtcNow).Returns(today);

        var yesterdayRequest = new SetPenaltyAmountRequest(200m, today.AddDays(-1));

        await Assert.ThrowsAsync<InvalidPenaltyEffectiveDateException>(() =>
            SetPenaltyAmountEndpoint.HandleAsync(
                violationType.Id.Value,
                yesterdayRequest,
                db,
                dateTimeMock.Object,
                CancellationToken.None));
    }

    [Fact]
    public async Task SetPenaltyAmount_WhenViolationTypeDoesNotExist_ThrowsViolationTypeNotFoundExceptionWithIdInMessage()
    {
        await using var db = CreateInMemoryDbContext();
        var missingId = Guid.NewGuid();
        var request = new SetPenaltyAmountRequest(150m, DateTime.UtcNow.AddDays(1));

        var ex = await Assert.ThrowsAsync<ViolationTypeNotFoundException>(() =>
            SetPenaltyAmountEndpoint.HandleAsync(missingId, request, db, null, CancellationToken.None));

        ex.Message.Should().Contain(missingId.ToString());
    }

    [Fact]
    public async Task CreateViolationType_WhenDuplicateNameExistsCaseInsensitive_ThrowsDuplicateViolationTypeNameException()
    {
        await using var db = CreateInMemoryDbContext();

        await CreateViolationTypeEndpoint.HandleAsync(new CreateViolationTypeRequest("Zajęcie dwóch miejsc", null), db, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateViolationTypeNameException>(() =>
            CreateViolationTypeEndpoint.HandleAsync(new CreateViolationTypeRequest("  zajęcie dwóch miejsc  ", "Opis"), db, CancellationToken.None));
    }

    [Theory]
    [InlineData(15, 45)]   // Nachodzi na koniec [0, 30]
    [InlineData(0, 10)]    // Zawiera się wewnątrz [0, 30]
    [InlineData(30, 60)]   // Punkt styku 30 min jest wspólny z [0, 30]
    [InlineData(20, null)] // Przedział otwarty nachodzący na [0, 30]
    public async Task CreateSurchargeTier_WhenRangeOverlapsWithExistingTier_ThrowsOverlappingSurchargeTierException(int newMin, int? newMax)
    {
        await using var db = CreateInMemoryDbContext();

        // Istniejący próg: 0 do 30 minut
        await CreateSurchargeTierEndpoint.HandleAsync(new CreateSurchargeTierRequest(0, 30, 20m), db, CancellationToken.None);

        // Próba dodania nakładającego się progu
        await Assert.ThrowsAsync<OverlappingSurchargeTierException>(() =>
            CreateSurchargeTierEndpoint.HandleAsync(new CreateSurchargeTierRequest(newMin, newMax, 50m), db, CancellationToken.None));
    }
}

