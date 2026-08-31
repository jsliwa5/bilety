namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Domain;
using PTickets.Shared;

public class ZoneTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        var zone = Zone.Create("Strefa A");

        Assert.NotEqual(default, zone.Id);
        Assert.Equal("Strefa A", zone.Name);
        Assert.NotNull(zone.Streets);
        Assert.Empty(zone.Streets);
        Assert.NotNull(zone.Exclusions);
        Assert.Empty(zone.Exclusions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => Zone.Create(name!));
    }
}
