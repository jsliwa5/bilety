namespace PTickets.Modules.Violations.Tests;

using PTickets.Modules.Violations.Domain;
using PTickets.Shared;

public class ViolationTypeTests
{
    [Fact]
    public void Create_WithValidNameAndDescription_Succeeds()
    {
        var name = "No Parking";
        var description = "Parked in a designated no-parking zone";

        var violationType = ViolationType.Create(name, description);

        Assert.NotNull(violationType);
        Assert.NotEqual(ViolationTypeId.Empty, violationType.Id);
        Assert.Equal(name, violationType.Name);
        Assert.Equal(description, violationType.Description);
        Assert.Empty(violationType.PenaltyAmounts);
    }

    [Fact]
    public void Create_WithValidNameAndNullDescription_Succeeds()
    {
        var name = "Expired Ticket";

        var violationType = ViolationType.Create(name);

        Assert.NotNull(violationType);
        Assert.NotEqual(ViolationTypeId.Empty, violationType.Id);
        Assert.Equal(name, violationType.Name);
        Assert.Null(violationType.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceOrNullName_ThrowsArgumentException(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => ViolationType.Create(name!));
    }

    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var name = "Overtime Parking";
        var description = "Exceeded maximum allowed parking duration";

        var violationType = ViolationType.Create(name, description);

        Assert.NotEqual(Guid.Empty, violationType.Id.Value);
        Assert.Equal(name, violationType.Name);
        Assert.Equal(description, violationType.Description);
        Assert.NotNull(violationType.PenaltyAmounts);
        Assert.Empty(violationType.PenaltyAmounts);
    }

    [Fact]
    public void AddPenaltyAmount_ValidAmount_AddsToPenaltyAmountsCollection()
    {
        var violationType = ViolationType.Create("No Parking");
        var amount = 100.00m;
        var effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var penaltyAmount = violationType.AddPenaltyAmount(amount, effectiveFrom);

        Assert.NotNull(penaltyAmount);
        Assert.Single(violationType.PenaltyAmounts);
        Assert.Contains(penaltyAmount, violationType.PenaltyAmounts);
        Assert.Equal(violationType.Id, penaltyAmount.ViolationTypeId);
        Assert.Equal(amount, penaltyAmount.Amount);
        Assert.Equal(effectiveFrom, penaltyAmount.EffectiveFrom);
    }
}
