namespace PTickets.Modules.Violations.Tests;

using PTickets.Modules.Violations.Common.Data;
using PTickets.Shared;

public class SurchargeTierTests
{
    [Fact]
    public void Create_WithValidParameters_BoundedTier_Succeeds()
    {
        var tier = SurchargeTier.Create(0, 30, 15.00m);

        Assert.NotNull(tier);
        Assert.NotEqual(PenaltyTierId.Empty, tier.Id);
        Assert.Equal(0, tier.MinMinutes);
        Assert.Equal(30, tier.MaxMinutes);
        Assert.Equal(15.00m, tier.Amount);
    }

    [Fact]
    public void Create_WithValidParameters_OpenEndedTier_Succeeds()
    {
        var tier = SurchargeTier.Create(61, null, 50.00m);

        Assert.NotNull(tier);
        Assert.NotEqual(PenaltyTierId.Empty, tier.Id);
        Assert.Equal(61, tier.MinMinutes);
        Assert.Null(tier.MaxMinutes);
        Assert.Equal(50.00m, tier.Amount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithNegativeMinMinutes_ThrowsCustomException(int minMinutes)
    {
        var exception = Assert.Throws<PTickets.Modules.Violations.Common.Exceptions.InvalidSurchargeTierMinMinutesException>(() =>
            SurchargeTier.Create(minMinutes, 30, 20.00m));


    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-50)]
    public void Create_WithZeroOrNegativeAmount_ThrowsCustomException(decimal amount)
    {
        var exception = Assert.Throws<PTickets.Modules.Violations.Common.Exceptions.InvalidSurchargeTierAmountException>(() =>
            SurchargeTier.Create(0, 30, amount));


    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(30, 29)]
    [InlineData(30, 0)]
    public void Create_WithMaxMinutesLessThanOrEqualToMinMinutes_ThrowsCustomException(int minMinutes, int maxMinutes)
    {
        var exception = Assert.Throws<PTickets.Modules.Violations.Common.Exceptions.InvalidSurchargeTierMaxMinutesException>(() =>
            SurchargeTier.Create(minMinutes, maxMinutes, 20.00m));


    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    public void Matches_WhenOvertimeFallsWithinRange_ReturnsTrue(int overtimeMinutes)
    {
        var tier = SurchargeTier.Create(15, 60, 25.00m);

        var matches = tier.Matches(overtimeMinutes);

        Assert.True(matches);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(14)]
    [InlineData(61)]
    [InlineData(100)]
    public void Matches_WhenOvertimeIsOutsideRange_ReturnsFalse(int overtimeMinutes)
    {
        var tier = SurchargeTier.Create(15, 60, 25.00m);

        var matches = tier.Matches(overtimeMinutes);

        Assert.False(matches);
    }

    [Theory]
    [InlineData(59, false)]
    [InlineData(60, true)]
    [InlineData(61, true)]
    [InlineData(120, true)]
    [InlineData(1000, true)]
    public void Matches_WithNullMaxMinutes_MatchesAnyOvertimeGreaterThanOrEqualToMinMinutes(int overtimeMinutes, bool expected)
    {
        var tier = SurchargeTier.Create(60, null, 50.00m);

        var matches = tier.Matches(overtimeMinutes);

        Assert.Equal(expected, matches);
    }

    [Theory]
    [InlineData(0, 10.00)]
    [InlineData(15, 10.00)]
    [InlineData(30, 10.00)]
    [InlineData(31, 25.00)]
    [InlineData(45, 25.00)]
    [InlineData(60, 25.00)]
    [InlineData(61, 50.00)]
    [InlineData(120, 50.00)]
    public void Matches_MultipleTiersCanCoverDifferentRangesWithoutOverlap(int overtimeMinutes, decimal expectedAmount)
    {
        var tiers = new List<SurchargeTier>
        {
            SurchargeTier.Create(0, 30, 10.00m),
            SurchargeTier.Create(31, 60, 25.00m),
            SurchargeTier.Create(61, null, 50.00m)
        };

        var matchingTiers = tiers.Where(t => t.Matches(overtimeMinutes)).ToList();

        Assert.Single(matchingTiers);
        Assert.Equal(expectedAmount, matchingTiers[0].Amount);
    }
}
