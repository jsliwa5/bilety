namespace PTickets.Modules.Violations.Tests;

using FluentAssertions;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations.Common.Exceptions;
using PTickets.Shared;
using Xunit;

public class PenaltyAmountTests
{
    private static readonly DateTime TodayUtc = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidAmountAndTodayDate_Succeeds()
    {
        var violationTypeId = ViolationTypeId.New();
        var amount = 75.50m;
        var effectiveFrom = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc); // Ten sam dzień rano

        var penaltyAmount = PenaltyAmount.Create(violationTypeId, amount, effectiveFrom, TodayUtc);

        penaltyAmount.Should().NotBeNull();
        penaltyAmount.Id.Should().NotBe(Guid.Empty);
        penaltyAmount.ViolationTypeId.Should().Be(violationTypeId);
        penaltyAmount.Amount.Should().Be(amount);
        penaltyAmount.EffectiveFrom.Should().Be(effectiveFrom);
    }

    [Fact]
    public void Create_WithFutureEffectiveFromDate_Succeeds()
    {
        var violationTypeId = ViolationTypeId.New();
        var futureDate = TodayUtc.AddDays(7);

        var penaltyAmount = PenaltyAmount.Create(violationTypeId, 150.00m, futureDate, TodayUtc);

        penaltyAmount.Amount.Should().Be(150.00m);
        penaltyAmount.EffectiveFrom.Should().Be(futureDate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-7)]
    [InlineData(-365)]
    public void Create_WithEffectiveFromDateEarlierThanToday_ThrowsInvalidPenaltyEffectiveDateException(int daysOffset)
    {
        var violationTypeId = ViolationTypeId.New();
        var pastDate = TodayUtc.AddDays(daysOffset);

        Assert.Throws<InvalidPenaltyEffectiveDateException>(() =>
            PenaltyAmount.Create(violationTypeId, 150.00m, pastDate, TodayUtc));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeAmount_ThrowsInvalidPenaltyAmountException(decimal amount)
    {
        var violationTypeId = ViolationTypeId.New();

        Assert.Throws<InvalidPenaltyAmountException>(() =>
            PenaltyAmount.Create(violationTypeId, amount, TodayUtc, TodayUtc));
    }
}

