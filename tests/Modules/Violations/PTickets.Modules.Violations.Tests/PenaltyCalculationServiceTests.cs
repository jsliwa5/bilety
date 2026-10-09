using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PTickets.Modules.Violations;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations;
using PTickets.Shared;
using PTickets.Shared.Abstractions;
using Xunit;

namespace PTickets.Modules.Violations.Tests;

public class PenaltyCalculationServiceTests
{
    private readonly ViolationsDbContext _dbContext;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly PenaltyCalculationService _service;

    public PenaltyCalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ViolationsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ViolationsDbContext(options);
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _service = new PenaltyCalculationService(_dbContext, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnZero_WhenNoTiersExist()
    {
        // Act
        var result = await _service.CalculateSurchargeAsync(10);

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnAmount_WhenMatchingTierExists()
    {
        // Arrange
        var tier = SurchargeTier.Create(0, 15, 50m);
        _dbContext.SurchargeTiers.Add(tier);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateSurchargeAsync(10);

        // Assert
        result.Should().Be(50m);
    }

    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnZero_WhenNoMatchingTierExists()
    {
        // Arrange
        var tier = SurchargeTier.Create(16, 30, 100m);
        _dbContext.SurchargeTiers.Add(tier);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateSurchargeAsync(10); // 10 < 16, no match

        // Assert
        result.Should().Be(0m);
    }
    
    [Fact]
    public async Task CalculateSurchargeAsync_ShouldReturnAmount_WhenOvertimeMatchesTierWithoutMax()
    {
        // Arrange
        var tier1 = SurchargeTier.Create(0, 60, 50m);
        var tier2 = SurchargeTier.Create(61, null, 150m);
        _dbContext.SurchargeTiers.AddRange(tier1, tier2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateSurchargeAsync(120);

        // Assert
        result.Should().Be(150m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnZero_WhenNoPenaltyExistsForViolationType()
    {
        // Act
        var result = await _service.GetCurrentPenaltyAmountAsync(ViolationTypeId.New());

        // Assert
        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnAmount_WhenActivePenaltyExists()
    {
        // Arrange
        var violationTypeId = ViolationTypeId.New();
        var now = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(now);

        var penalty = PenaltyAmount.Create(violationTypeId, 250m, now.AddDays(-1));
        _dbContext.PenaltyAmounts.Add(penalty);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);

        // Assert
        result.Should().Be(250m);
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnMostRecentActivePenalty_WhenMultiplePenaltiesExist()
    {
        // Arrange
        var violationTypeId = ViolationTypeId.New();
        var now = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(now);

        var penaltyOld = PenaltyAmount.Create(violationTypeId, 200m, now.AddDays(-10));
        var penaltyNew = PenaltyAmount.Create(violationTypeId, 300m, now.AddDays(-2));
        var penaltyFuture = PenaltyAmount.Create(violationTypeId, 400m, now.AddDays(2)); // Not active yet

        _dbContext.PenaltyAmounts.AddRange(penaltyOld, penaltyNew, penaltyFuture);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);

        // Assert
        result.Should().Be(300m); // Should pick penaltyNew
    }

    [Fact]
    public async Task GetCurrentPenaltyAmountAsync_ShouldReturnZero_WhenOnlyFuturePenaltiesExist()
    {
        // Arrange
        var violationTypeId = ViolationTypeId.New();
        var now = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(now);

        var penaltyFuture = PenaltyAmount.Create(violationTypeId, 400m, now.AddDays(2));

        _dbContext.PenaltyAmounts.Add(penaltyFuture);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentPenaltyAmountAsync(violationTypeId);

        // Assert
        result.Should().Be(0m);
    }
}
