namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Data;
using PTickets.Shared;

public class ZoneTests
{
    [Fact]
    public void CreateMultiStreet_WithValidName_ShouldSucceedWithEmptyStreets()
    {
        var zone = Zone.CreateMultiStreet("Strefa A", null);

        Assert.NotEqual(default, zone.Id);
        Assert.Equal("Strefa A", zone.Name);
        Assert.Equal(ZoneType.MultiStreet, zone.Type);
        Assert.NotNull(zone.Streets);
        Assert.Empty(zone.Streets);
        Assert.NotNull(zone.Exclusions);
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

    [Fact]
    public void Create_WithSingleType_ShouldDelegateToCreateSingle()
    {
        var zone = Zone.Create("Strefa B", ZoneType.Single, null);

        Assert.Equal(ZoneType.Single, zone.Type);
        Assert.Single(zone.Streets);
        Assert.True(zone.Streets.First().RepresentsWholeZone);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => Zone.Create(name!, ZoneType.MultiStreet, null));
        Assert.Throws<ArgumentException>(() => Zone.CreateSingle(name!, null));
        Assert.Throws<ArgumentException>(() => Zone.CreateMultiStreet(name!, null));
    }

    [Fact]
    public void AddStreet_ToMultiStreetZone_ShouldSucceed()
    {
        var zone = Zone.CreateMultiStreet("Strefa A", null);

        var street = zone.AddStreet("Marszałkowska", null);

        Assert.Single(zone.Streets);
        Assert.Contains(street, zone.Streets);
        Assert.Equal("Marszałkowska", street.Name);
        Assert.Equal(zone.Id, street.ZoneId);
        Assert.False(street.RepresentsWholeZone);
    }

    [Fact]
    public void AddStreet_ToSingleZone_ShouldThrowInvalidOperationException()
    {
        var zone = Zone.CreateSingle("Strefa Centrum", null);

        Assert.Throws<InvalidOperationException>(() => zone.AddStreet("Nowa", null));
    }

    [Fact]
    public void AddExclusion_ToZone_ShouldAddExclusion()
    {
        var zone = Zone.CreateMultiStreet("Strefa A", null);
        var start = DateTime.UtcNow;
        var end = start.AddDays(2);

        var exclusion = zone.AddExclusion(start, end, "Remont");

        Assert.Single(zone.Exclusions);
        Assert.Contains(exclusion, zone.Exclusions);
        Assert.Equal(zone.Id, exclusion.ZoneId);
        Assert.Equal("Remont", exclusion.Reason);
    }
}
