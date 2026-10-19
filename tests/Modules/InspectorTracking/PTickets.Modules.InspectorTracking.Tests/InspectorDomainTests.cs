namespace PTickets.Modules.InspectorTracking.Tests;

using FluentAssertions;
using PTickets.Modules.InspectorTracking.Common.Data;
using PTickets.Modules.InspectorTracking.Common.Exceptions;
using PTickets.Shared;
using Xunit;

public class InspectorDomainTests
{
    [Fact]
    public void Create_WithValidNames_TrimsWhitespaceAndInitializesUnassigned()
    {
        var inspector = Inspector.Create("  Jan  ", "  Kowalski  ");
        inspector.Id.Should().NotBe(InspectorId.Empty);
        inspector.FirstName.Should().Be("Jan");
        inspector.LastName.Should().Be("Kowalski");
        inspector.AssignedToZone.Should().BeFalse();
        inspector.ZoneId.Should().BeNull();
        inspector.LocationLogs.Should().BeEmpty();
        inspector.InspectionAttempts.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidFirstName_ThrowsInvalidInspectorFirstNameException(string? invalidFirstName)
    {
        Assert.Throws<InvalidInspectorFirstNameException>(() =>
            Inspector.Create(invalidFirstName!, "Kowalski"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidLastName_ThrowsInvalidInspectorLastNameException(string? invalidLastName)
    {
        Assert.Throws<InvalidInspectorLastNameException>(() =>
            Inspector.Create("Jan", invalidLastName!));
    }

    [Fact]
    public void Restore_WithEmptyInspectorId_ThrowsInvalidInspectorIdException()
    {
        Assert.Throws<InvalidInspectorIdException>(() =>
            Inspector.Restore(InspectorId.Empty, "Jan", "Kowalski", false, null, [], []));
    }

    [Fact]
    public void AssignToZone_WithEmptyZoneId_ThrowsInvalidZoneAssignmentException()
    {
        var inspector = Inspector.Create("Jan", "Kowalski");
        Assert.Throws<InvalidZoneAssignmentException>(() =>
            inspector.AssignToZone(ZoneId.Empty));
    }

    [Fact]
    public void RegisterLogs_WhenNullPassed_ThrowsMissingLogExceptions()
    {
        var inspector = Inspector.Create("Jan", "Kowalski");
        Assert.Throws<MissingLocationLogException>(() => inspector.RegisterLocationLog(null!));
        Assert.Throws<MissingInspectionLogException>(() => inspector.RegisterInspectionAttempt(null!));
    }

    [Theory]
    [InlineData(-90.001, 21.0)]
    [InlineData(90.001, 21.0)]
    [InlineData(52.0, -180.001)]
    [InlineData(52.0, 180.001)]
    public void LocationLog_WithCoordinatesOutOfBounds_ThrowsInvalidLocationCoordinatesException(double lat, double lng)
    {
        Assert.Throws<InvalidLocationCoordinatesException>(() =>
            new LocationLog(InspectorId.New(), DateTime.UtcNow, lat, lng));
    }
}

