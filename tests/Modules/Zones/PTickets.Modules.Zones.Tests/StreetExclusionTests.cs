namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Domain;
using PTickets.Shared;

public class StreetExclusionTests
{
    [Fact]
    public void Create_WithValidDates_ShouldSucceed()
    {
        var streetId = StreetId.New();
        var startDate = new DateTime(2026, 9, 1, 0, 0, 0);
        var endDate = new DateTime(2026, 9, 10, 23, 59, 59);
        var reason = "Awaria wodociągowa";

        var exclusion = StreetExclusion.Create(streetId, startDate, endDate, reason);

        Assert.NotEqual(Guid.Empty, exclusion.Id);
        Assert.Equal(streetId, exclusion.StreetId);
        Assert.Equal(startDate, exclusion.StartDate);
        Assert.Equal(endDate, exclusion.EndDate);
        Assert.Equal(reason, exclusion.Reason);
    }

    [Theory]
    [InlineData("2026-09-10T12:00:00", "2026-09-10T12:00:00")]
    [InlineData("2026-09-11T12:00:00", "2026-09-10T12:00:00")]
    public void Create_WithStartDateEqualOrAfterEndDate_ShouldThrowArgumentException(string startStr, string endStr)
    {
        var streetId = StreetId.New();
        var startDate = DateTime.Parse(startStr);
        var endDate = DateTime.Parse(endStr);

        Assert.Throws<ArgumentException>(() => StreetExclusion.Create(streetId, startDate, endDate, "Awaria"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceReason_ShouldThrowArgumentException(string? reason)
    {
        var streetId = StreetId.New();
        var startDate = new DateTime(2026, 9, 1);
        var endDate = new DateTime(2026, 9, 10);

        Assert.Throws<ArgumentException>(() => StreetExclusion.Create(streetId, startDate, endDate, reason!));
    }

    [Fact]
    public void IsActiveAt_WhenWithinRange_ShouldReturnTrue()
    {
        var streetId = StreetId.New();
        var startDate = new DateTime(2026, 9, 1, 8, 0, 0);
        var endDate = new DateTime(2026, 9, 5, 18, 0, 0);
        var exclusion = StreetExclusion.Create(streetId, startDate, endDate, "Awaria");

        var testDate = new DateTime(2026, 9, 3, 12, 0, 0);

        Assert.True(exclusion.IsActiveAt(testDate));
    }

    [Fact]
    public void IsActiveAt_WhenBeforeOrAfterRange_ShouldReturnFalse()
    {
        var streetId = StreetId.New();
        var startDate = new DateTime(2026, 9, 1, 8, 0, 0);
        var endDate = new DateTime(2026, 9, 5, 18, 0, 0);
        var exclusion = StreetExclusion.Create(streetId, startDate, endDate, "Awaria");

        var beforeDate = new DateTime(2026, 9, 1, 7, 59, 59);
        var afterDate = new DateTime(2026, 9, 5, 18, 0, 1);

        Assert.False(exclusion.IsActiveAt(beforeDate));
        Assert.False(exclusion.IsActiveAt(afterDate));
    }

    [Fact]
    public void IsActiveAt_ExactlyAtBoundaries_ShouldReturnTrue()
    {
        var streetId = StreetId.New();
        var startDate = new DateTime(2026, 9, 1, 8, 0, 0);
        var endDate = new DateTime(2026, 9, 5, 18, 0, 0);
        var exclusion = StreetExclusion.Create(streetId, startDate, endDate, "Awaria");

        Assert.True(exclusion.IsActiveAt(startDate));
        Assert.True(exclusion.IsActiveAt(endDate));
    }
}
