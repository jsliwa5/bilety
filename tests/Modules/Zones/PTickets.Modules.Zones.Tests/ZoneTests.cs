namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Common.Data;
using PTickets.Modules.Zones.Common.Exceptions;

public class ZoneTests
{
    [Fact]
    public void CreateMultiStreet_WithValidName_ShouldSucceedWithEmptyStreets()
    {
        var zone = Zone.CreateMultiStreet("  Strefa A  ", null);

        Assert.NotEqual(default, zone.Id);
        Assert.Equal("Strefa A", zone.Name);
        Assert.Equal(ZoneType.MultiStreet, zone.Type);
        Assert.Empty(zone.Streets);
        Assert.Empty(zone.Exclusions);
    }

    [Fact]
    public void CreateSingle_WithValidName_ShouldCreateRepresentativeStreetAutomatically()
    {
        var zone = Zone.CreateSingle("Strefa Centrum", null);

        Assert.NotEqual(default, zone.Id);
        Assert.Equal("Strefa Centrum", zone.Name);
        Assert.Equal(ZoneType.Single, zone.Type);
        Assert.Single(zone.Streets);

        var street = zone.Streets.First();
        Assert.Equal("Strefa Centrum", street.Name);
        Assert.Equal(zone.Id, street.ZoneId);
        Assert.True(street.RepresentsWholeZone);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public void Create_WithInvalidZoneType_ShouldThrowInvalidZoneTypeException(int invalidType)
    {
        Assert.Throws<InvalidZoneTypeException>(() =>
            Zone.Create("Strefa X", (ZoneType)invalidType, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowInvalidZoneNameException(string? name)
    {
        Assert.Throws<InvalidZoneNameException>(() => Zone.Create(name!, ZoneType.MultiStreet, null));
        Assert.Throws<InvalidZoneNameException>(() => Zone.CreateSingle(name!, null));
        Assert.Throws<InvalidZoneNameException>(() => Zone.CreateMultiStreet(name!, null));
    }

    [Fact]
    public void AddStreet_ToMultiStreetZone_ShouldCreateNormalStreetWithRepresentsWholeZoneFalse()
    {
        var zone = Zone.CreateMultiStreet("Strefa A", null);

        var street = zone.AddStreet("  Marszałkowska  ", null);

        Assert.Single(zone.Streets);
        Assert.Equal("Marszałkowska", street.Name);
        Assert.Equal(zone.Id, street.ZoneId);
        Assert.False(street.RepresentsWholeZone);
    }

    [Fact]
    public void AddStreet_ToSingleZone_ShouldThrowCannotAddStreetToSingleZoneException()
    {
        var zone = Zone.CreateSingle("Strefa Centrum", null);

        Assert.Throws<CannotAddStreetToSingleZoneException>(() => zone.AddStreet("Nowa", null));
    }

    [Fact]
    public void AddExclusion_ToZone_ShouldAddExclusion()
    {
        var zone = Zone.CreateMultiStreet("Strefa A", null);
        var start = DateTime.UtcNow;
        var end = start.AddDays(2);

        var exclusion = zone.AddExclusion(start, end, "Remont");

        Assert.Single(zone.Exclusions);
        Assert.Equal(zone.Id, exclusion.ZoneId);
        Assert.Equal("Remont", exclusion.Reason);
    }
}

