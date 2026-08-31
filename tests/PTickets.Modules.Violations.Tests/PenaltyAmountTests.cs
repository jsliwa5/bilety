namespace PTickets.Modules.Violations.Tests;

using PTickets.Modules.Violations.Domain;
using PTickets.Shared;

public class PenaltyAmountTests
{
    [Fact]
    public void Create_WithValidAmount_Succeeds()
    {
        var violationTypeId = ViolationTypeId.New();
        var amount = 75.50m;
        var effectiveFrom = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var penaltyAmount = PenaltyAmount.Create(violationTypeId, amount, effectiveFrom);

        Assert.NotNull(penaltyAmount);
        Assert.NotEqual(Guid.Empty, penaltyAmount.Id);
        Assert.Equal(violationTypeId, penaltyAmount.ViolationTypeId);
        Assert.Equal(amount, penaltyAmount.Amount);
        Assert.Equal(effectiveFrom, penaltyAmount.EffectiveFrom);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeAmount_ThrowsArgumentOutOfRangeException(decimal amount)
    {
        var violationTypeId = ViolationTypeId.New();
        var effectiveFrom = DateTime.UtcNow;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            PenaltyAmount.Create(violationTypeId, amount, effectiveFrom));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var violationTypeId = ViolationTypeId.New();
        var amount = 150.00m;
        var effectiveFrom = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var penaltyAmount = PenaltyAmount.Create(violationTypeId, amount, effectiveFrom);

        Assert.NotEqual(Guid.Empty, penaltyAmount.Id);
        Assert.Equal(violationTypeId, penaltyAmount.ViolationTypeId);
        Assert.Equal(150.00m, penaltyAmount.Amount);
        Assert.Equal(effectiveFrom, penaltyAmount.EffectiveFrom);
    }
}
