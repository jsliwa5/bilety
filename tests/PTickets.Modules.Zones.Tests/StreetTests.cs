namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Domain;
using PTickets.Shared;

public class StreetTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var zoneId = ZoneId.New();
        var schedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]);

        var street = Street.Create(zoneId, "Marszałkowska", representsWholeZone: true, schedule: schedule);

        Assert.NotEqual(default, street.Id);
        Assert.Equal(zoneId, street.ZoneId);
        Assert.Equal("Marszałkowska", street.Name);
        Assert.True(street.RepresentsWholeZone);
        Assert.Equal(schedule, street.PaidParkingSchedule);
    }

    [Fact]
    public void Create_WithDefaultParameters_ShouldSucceed()
    {
        var zoneId = ZoneId.New();

        var street = Street.Create(zoneId, "Floriańska");

        Assert.NotEqual(default, street.Id);
        Assert.Equal(zoneId, street.ZoneId);
        Assert.Equal("Floriańska", street.Name);
        Assert.False(street.RepresentsWholeZone);
        Assert.Null(street.PaidParkingSchedule);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string? name)
    {
        var zoneId = ZoneId.New();

        Assert.Throws<ArgumentException>(() => Street.Create(zoneId, name!));
    }

    [Fact]
    public void IsPaidAt_WithSchedule_ShouldReturnExpectedResults()
    {
        var zoneId = ZoneId.New();
        var schedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]);
        var street = Street.Create(zoneId, "Marszałkowska", schedule: schedule);

        var mondayMidday = new DateTime(2026, 8, 31, 12, 0, 0); // Monday 12:00
        var mondayNight = new DateTime(2026, 8, 31, 20, 0, 0);  // Monday 20:00
        var sundayMidday = new DateTime(2026, 8, 30, 12, 0, 0); // Sunday 12:00

        Assert.True(street.IsPaidAt(mondayMidday));
        Assert.False(street.IsPaidAt(mondayNight));
        Assert.False(street.IsPaidAt(sundayMidday));
    }

    [Fact]
    public void IsPaidAt_WithoutSchedule_ShouldReturnFalse()
    {
        var zoneId = ZoneId.New();
        var street = Street.Create(zoneId, "Floriańska", schedule: null);

        var mondayMidday = new DateTime(2026, 8, 31, 12, 0, 0);

        Assert.False(street.IsPaidAt(mondayMidday));
    }
}
