namespace PTickets.Modules.Violations.Tests;

using PTickets.Modules.Violations.Domain;
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
    public void Create_WithNegativeMinMinutes_ThrowsArgumentOutOfRangeException(int minMinutes)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            SurchargeTier.Create(minMinutes, 30, 20.00m));

        Assert.Equal("minMinutes", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-50)]
    public void Create_WithZeroOrNegativeAmount_ThrowsArgumentOutOfRangeException(decimal amount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            SurchargeTier.Create(0, 30, amount));

        Assert.Equal("amount", exception.ParamName);
    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(30, 29)]
    [InlineData(30, 0)]
    public void Create_WithMaxMinutesLessThanOrEqualToMinMinutes_ThrowsArgumentException(int minMinutes, int maxMinutes)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            SurchargeTier.Create(minMinutes, maxMinutes, 20.00m));

        Assert.Equal("maxMinutes", exception.ParamName);
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
