namespace PTickets.Modules.Zones.Tests;

using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PTickets.Modules.Zones;
using PTickets.Modules.Zones.Common.Data;
using PTickets.Modules.Zones.Common.Exceptions;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.CreateStreet;
using PTickets.Modules.Zones.CreateZone;
using PTickets.Shared;

public class ZonesUseCasesAndFacadeTests
{
    private static ZonesDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZonesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ZonesDbContext(options);
    }

    [Fact]
    public async Task CreateZone_WhenSingleZone_PublishesZoneCreatedAndStreetCreatedEvents()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var mediatorMock = new Mock<IMediator>();

        var request = new CreateZoneRequest(
            "Strefa Stare Miasto",
            ZoneType.Single,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday]);

        // Act
        await CreateZoneEndpoint.HandleAsync(request, db, mediatorMock.Object, CancellationToken.None);

        // Assert
        var savedZone = await db.Zones.Include(z => z.Streets).SingleAsync();
        savedZone.Type.Should().Be(ZoneType.Single);
        savedZone.Streets.Should().ContainSingle(s => s.RepresentsWholeZone && s.Name == "Strefa Stare Miasto");

        mediatorMock.Verify(m => m.Publish(It.IsAny<ZoneCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        mediatorMock.Verify(m => m.Publish(It.IsAny<StreetCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateZone_WhenMultiStreetZone_PublishesOnlyZoneCreatedEvent()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var mediatorMock = new Mock<IMediator>();

        var request = new CreateZoneRequest(
            "Strefa Centrum",
            ZoneType.MultiStreet,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday]);

        // Act
        await CreateZoneEndpoint.HandleAsync(request, db, mediatorMock.Object, CancellationToken.None);

        // Assert
        mediatorMock.Verify(m => m.Publish(It.IsAny<ZoneCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        mediatorMock.Verify(m => m.Publish(It.IsAny<StreetCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStreet_WhenPartialScheduleProvided_ThrowsInvalidParkingScheduleException()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var mediatorMock = new Mock<IMediator>();

        var zone = Zone.CreateMultiStreet(
            "Strefa A",
            new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]));
        await db.Zones.AddAsync(zone);
        await db.SaveChangesAsync();

        // Podano StartTime i EndTime, ale brak PaidDays (null)
        var invalidRequest1 = new CreateStreetRequest("Marszałkowska", new TimeOnly(9, 0), new TimeOnly(17, 0), null);

        // Podano tylko StartTime, brak EndTime
        var invalidRequest2 = new CreateStreetRequest("Marszałkowska", new TimeOnly(9, 0), null, [DayOfWeek.Monday]);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidParkingScheduleException>(() =>
            CreateStreetEndpoint.HandleAsync(zone.Id.Value, invalidRequest1, db, mediatorMock.Object, CancellationToken.None));

        await Assert.ThrowsAsync<InvalidParkingScheduleException>(() =>
            CreateStreetEndpoint.HandleAsync(zone.Id.Value, invalidRequest2, db, mediatorMock.Object, CancellationToken.None));
    }

    [Fact]
    public async Task CreateStreet_WhenZoneDoesNotExist_ThrowsZoneNotFoundException()
    {
        await using var db = CreateInMemoryDbContext();
        var mediatorMock = new Mock<IMediator>();

        var request = new CreateStreetRequest("Marszałkowska");

        await Assert.ThrowsAsync<ZoneNotFoundException>(() =>
            CreateStreetEndpoint.HandleAsync(Guid.NewGuid(), request, db, mediatorMock.Object, CancellationToken.None));
    }

    [Fact]
    public async Task IsPaidAtAsync_WhenStreetHasNoSchedule_InheritsZoneSchedule()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var zoneSchedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]);
        var zone = Zone.CreateMultiStreet("Strefa A", zoneSchedule);
        var street = zone.AddStreet("Marszałkowska", schedule: null);
        await db.Zones.AddAsync(zone);
        await db.SaveChangesAsync();

        var facade = new ZonesModuleFacade(db);

        // Act & Assert
        var mondayMidday = new DateTime(2026, 8, 31, 12, 0, 0); // Poniedziałek 12:00
        var saturdayMidday = new DateTime(2026, 8, 29, 12, 0, 0); // Sobota 12:00

        (await facade.IsPaidAtAsync(street.Id, mondayMidday, CancellationToken.None)).Should().BeTrue();
        (await facade.IsPaidAtAsync(street.Id, saturdayMidday, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task IsPaidAtAsync_WhenStreetOverridesSchedule_UsesStreetScheduleInsteadOfZoneSchedule()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var zoneSchedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]);
        var streetSchedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(22, 0), [DayOfWeek.Monday, DayOfWeek.Saturday]);
        var zone = Zone.CreateMultiStreet("Strefa A", zoneSchedule);
        var street = zone.AddStreet("Nowy Świat", streetSchedule);
        await db.Zones.AddAsync(zone);
        await db.SaveChangesAsync();

        var facade = new ZonesModuleFacade(db);

        // Act & Assert — Sobota 20:00 (w strefie bezpłatna, ale na ulicy Nowy Świat płatna)
        var saturdayEvening = new DateTime(2026, 8, 29, 20, 0, 0);

        (await facade.IsPaidAtAsync(street.Id, saturdayEvening, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task IsExcludedAsync_WhenZoneOrStreetHasActiveExclusion_ReturnsTrue()
    {
        // Arrange
        await using var db = CreateInMemoryDbContext();
        var zone = Zone.CreateMultiStreet("Strefa A", new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]));
        var street1 = zone.AddStreet("Ulica 1");
        var street2 = zone.AddStreet("Ulica 2");

        var start = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 9, 10, 23, 59, 59, DateTimeKind.Utc);

        // Wyłączenie tylko dla Ulicy 1
        var streetExclusion = StreetExclusion.Create(street1.Id, start, end, "Awaria rury");
        await db.Zones.AddAsync(zone);
        await db.StreetExclusions.AddAsync(streetExclusion);
        await db.SaveChangesAsync();

        var facade = new ZonesModuleFacade(db);
        var checkDate = new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert 1: Ulica 1 jest wyłączona, Ulica 2 nie jest
        (await facade.IsExcludedAsync(zone.Id, street1.Id, checkDate, CancellationToken.None)).Should().BeTrue();
        (await facade.IsExcludedAsync(zone.Id, street2.Id, checkDate, CancellationToken.None)).Should().BeFalse();

        // Act & Assert 2: Dodajemy wyłączenie dla całej Strefy A -> teraz Ulica 2 też staje się wyłączona
        var zoneExclusion = zone.AddExclusion(start, end, "Święto Miasta");
        await db.ZoneExclusions.AddAsync(zoneExclusion);
        await db.SaveChangesAsync();

        (await facade.IsExcludedAsync(zone.Id, street2.Id, checkDate, CancellationToken.None)).Should().BeTrue();
    }
}

