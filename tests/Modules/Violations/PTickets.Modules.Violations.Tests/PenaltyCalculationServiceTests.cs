using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Shared;
using PTickets.Shared.Abstractions;
using Xunit;

namespace PTickets.Modules.Violations.Tests;

public class PenaltyCalculationServiceTests
{
    private readonly ViolationsDbContext _dbContext;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly PenaltyCalculationService _service;
    private static readonly DateTime BaseCreationDate = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public PenaltyCalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ViolationsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ViolationsDbContext(options);
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(BaseCreationDate);

        _service = new PenaltyCalculationService(_dbContext, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnZero_WhenNoTiersExist()
    {
        var result = await _service.CalculateSurchargeAsync(10);
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnAmount_WhenMatchingTierExists()
    {
        var tier = SurchargeTier.Create(0, 15, 50m);
        _dbContext.SurchargeTiers.Add(tier);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CalculateSurchargeAsync(10);
        result.Should().Be(50m);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnZero_WhenNoMatchingTierExists()
    {
        var tier = SurchargeTier.Create(16, 30, 100m);
        _dbContext.SurchargeTiers.Add(tier);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CalculateSurchargeAsync(10);
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnAmount_WhenOvertimeMatchesTierWithoutMax()
    {
        var tier1 = SurchargeTier.Create(0, 60, 50m);
        var tier2 = SurchargeTier.Create(61, null, 150m);
        _dbContext.SurchargeTiers.AddRange(tier1, tier2);
        await _dbContext.SaveChangesAsync();

        var result = await _service.CalculateSurchargeAsync(120);
        result.Should().Be(150m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnZero_WhenNoPenaltyExistsForViolationType()
    {
        var result = await _service.GetCurrentPenaltyAmountAsync(ViolationTypeId.New());
        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnAmount_WhenSingleViolationHasActivePenalty()
    {
        var violationTypeId = ViolationTypeId.New();
        var penalty = PenaltyAmount.Create(violationTypeId, 250m, BaseCreationDate, BaseCreationDate);
        _dbContext.PenaltyAmounts.Add(penalty);
        await _dbContext.SaveChangesAsync();

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(BaseCreationDate.AddDays(1));

        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);
        result.Should().Be(250m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnMostRecentActivePenalty_WhenMultiplePenaltiesExist()
    {
        var violationTypeId = ViolationTypeId.New();

        var penaltyOld = PenaltyAmount.Create(violationTypeId, 200m, BaseCreationDate, BaseCreationDate);
        var penaltyNew = PenaltyAmount.Create(violationTypeId, 300m, BaseCreationDate.AddDays(8), BaseCreationDate);
        var penaltyFuture = PenaltyAmount.Create(violationTypeId, 400m, BaseCreationDate.AddDays(12), BaseCreationDate);

        _dbContext.PenaltyAmounts.AddRange(penaltyOld, penaltyNew, penaltyFuture);
        await _dbContext.SaveChangesAsync();

        // Sprawdzamy 10 dni po BaseCreationDate (penaltyNew już obowiązuje, penaltyFuture jeszcze nie)
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(BaseCreationDate.AddDays(10));

        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);
        result.Should().Be(300m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnZero_WhenOnlyFuturePenaltiesExist()
    {
        var violationTypeId = ViolationTypeId.New();

        var penaltyFuture = PenaltyAmount.Create(violationTypeId, 400m, BaseCreationDate.AddDays(2), BaseCreationDate);

        _dbContext.PenaltyAmounts.Add(penaltyFuture);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateTotalPenaltyAmountAsync_ForSingleViolation_ReturnsItsPenaltyAmount()
    {
        var type1 = ViolationTypeId.New();
        _dbContext.PenaltyAmounts.Add(PenaltyAmount.Create(type1, 150m, BaseCreationDate, BaseCreationDate));
        await _dbContext.SaveChangesAsync();

        var total = await _service.CalculateTotalPenaltyAmountAsync([type1]);
        total.Should().Be(150m);
    }

    [Fact]
    public async Task CalculateTotalPenaltyAmountAsync_ForMultipleDifferentViolations_ReturnsSumOfActivePenalties()
    {
        var noTicketType = ViolationTypeId.New();
        var twoSpotsType = ViolationTypeId.New();
        var blockedExitType = ViolationTypeId.New();

        _dbContext.PenaltyAmounts.AddRange(
            PenaltyAmount.Create(noTicketType, 150m, BaseCreationDate, BaseCreationDate),
            PenaltyAmount.Create(twoSpotsType, 100m, BaseCreationDate, BaseCreationDate),
            PenaltyAmount.Create(blockedExitType, 300m, BaseCreationDate, BaseCreationDate)
        );
        await _dbContext.SaveChangesAsync();

        var total = await _service.CalculateTotalPenaltyAmountAsync([noTicketType, twoSpotsType, blockedExitType]);
        total.Should().Be(550m);
    }

    [Fact]
    public async Task CalculateTotalPenaltyAmountAsync_WhenSameViolationOccursTwice_SumsEachOccurrence()
    {
        var type1 = ViolationTypeId.New();
        _dbContext.PenaltyAmounts.Add(PenaltyAmount.Create(type1, 120m, BaseCreationDate, BaseCreationDate));
        await _dbContext.SaveChangesAsync();

        var total = await _service.CalculateTotalPenaltyAmountAsync([type1, type1]);
        total.Should().Be(240m);
    }

    [Fact]
    public async Task CalculateTotalPenaltyAmountAsync_WhenListIsEmpty_ReturnsZero()
    {
        var total = await _service.CalculateTotalPenaltyAmountAsync([]);
        total.Should().Be(0m);
    }
}

