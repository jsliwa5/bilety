namespace PTickets.Modules.InspectorTracking.Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Modules.InspectorTracking.AddInspector;
using PTickets.Modules.InspectorTracking.AssignToZone;
using PTickets.Modules.InspectorTracking.Common.Data;
using PTickets.Modules.InspectorTracking.Common.Exceptions;
using PTickets.Modules.InspectorTracking.RegisterInspectionAttempt;
using PTickets.Modules.Zones.Contracts;
using PTickets.Shared;
using Xunit;

public class InspectorTrackingUseCasesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InspectorTrackingDbContext _dbContext;
    private readonly Mock<IZonesModule> _zonesModuleMock = new();

    public InspectorTrackingUseCasesTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<InspectorTrackingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new InspectorTrackingDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddInspector_WithValidRequest_PersistsInspectorAndFacadeConfirmsExistence()
    {
        var request = new AddInspectorRequest("Piotr", "Zieliński");

        var result = await AddInspectorEndpoint.AddInspector(request, _dbContext, CancellationToken.None);

        var createdResult = result.Should().BeOfType<Created<AddInspectorResponse>>().Subject;
        var inspectorId = new InspectorId(createdResult.Value!.Id);

        var facade = new InspectorTrackingModuleFacade(_dbContext);
        (await facade.InspectorExistsAsync(inspectorId, CancellationToken.None)).Should().BeTrue();
        (await facade.GetAssignedZoneAsync(inspectorId, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task AssignAndUnassignZone_FullLifecycle_UpdatesZoneAssignmentCorrectly()
    {
        // Arrange
        var inspector = Inspector.Create("Anna", "Nowak");
        var zoneA = ZoneId.New();
        var zoneB = ZoneId.New();

        await _dbContext.Inspectors.AddAsync(inspector);
        await _dbContext.SaveChangesAsync();

        _zonesModuleMock.Setup(z => z.ZoneExistsAsync(zoneA, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _zonesModuleMock.Setup(z => z.ZoneExistsAsync(zoneB, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var facade = new InspectorTrackingModuleFacade(_dbContext);

        // Act 1: Przypisanie do Strefy A
        await AssignToZoneEndpoint.AssignToZone(
            inspector.Id.Value,
            new AssignToZoneRequest(zoneA.Value),
            _dbContext,
            _zonesModuleMock.Object,
            CancellationToken.None);

        (await facade.GetAssignedZoneAsync(inspector.Id, CancellationToken.None)).Should().Be(zoneA);

        // Act 2: Zmiana przypisania ze Strefy A na Strefę B
        await AssignToZoneEndpoint.AssignToZone(
            inspector.Id.Value,
            new AssignToZoneRequest(zoneB.Value),
            _dbContext,
            _zonesModuleMock.Object,
            CancellationToken.None);

        (await facade.GetAssignedZoneAsync(inspector.Id, CancellationToken.None)).Should().Be(zoneB);

        // Act 3: Odpięcie ze strefy
        await AssignToZoneEndpoint.UnassignFromZone(inspector.Id.Value, _dbContext, CancellationToken.None);

        (await facade.GetAssignedZoneAsync(inspector.Id, CancellationToken.None)).Should().BeNull();
        var updatedInspector = await _dbContext.Inspectors.SingleAsync();
        updatedInspector.AssignedToZone.Should().BeFalse();
    }

    [Fact]
    public async Task AssignToZone_WhenZoneDoesNotExist_ThrowsInvalidZoneAssignmentException()
    {
        var inspector = Inspector.Create("Jan", "Kowalski");
        await _dbContext.Inspectors.AddAsync(inspector);
        await _dbContext.SaveChangesAsync();

        var nonExistentZoneId = ZoneId.New();
        _zonesModuleMock.Setup(z => z.ZoneExistsAsync(nonExistentZoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidZoneAssignmentException>(() =>
            AssignToZoneEndpoint.AssignToZone(
                inspector.Id.Value,
                new AssignToZoneRequest(nonExistentZoneId.Value),
                _dbContext,
                _zonesModuleMock.Object,
                CancellationToken.None));
    }

    [Fact]
    public async Task AssignOrUnassignZone_WhenInspectorDoesNotExist_ThrowsInspectorNotFoundException()
    {
        var missingInspectorId = Guid.NewGuid();

        await Assert.ThrowsAsync<InspectorNotFoundException>(() =>
            AssignToZoneEndpoint.AssignToZone(
                missingInspectorId,
                new AssignToZoneRequest(Guid.NewGuid()),
                _dbContext,
                _zonesModuleMock.Object,
                CancellationToken.None));

        await Assert.ThrowsAsync<InspectorNotFoundException>(() =>
            AssignToZoneEndpoint.UnassignFromZone(missingInspectorId, _dbContext, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterInspectionAttemptEventHandler_WhenInspectionStarted_RecordsLocationLogAndInspectionLog()
    {
        // Arrange
        var inspector = Inspector.Create("Marek", "Wiśniewski");
        await _dbContext.Inspectors.AddAsync(inspector);
        await _dbContext.SaveChangesAsync();

        var handler = new RegisterInspectionAttemptEventHandler(_dbContext);
        var inspectionId = InspectionId.New();
        var startedAt = new DateTime(2026, 10, 19, 13, 15, 0, DateTimeKind.Utc);
        var evt = new InspectionStartedEvent(inspectionId, inspector.Id, 50.0412, 21.9991, startedAt);

        // Act
        await handler.Handle(evt, CancellationToken.None);

        // Assert
        var locationLog = await _dbContext.LocationLogs.SingleAsync();
        locationLog.Latitude.Should().Be(50.0412);
        locationLog.Longitude.Should().Be(21.9991);
        locationLog.TimeOfLocation.Should().Be(startedAt);

        var inspectionLog = await _dbContext.InspectionLogs.SingleAsync();
        inspectionLog.InspectionId.Should().Be(inspectionId);
        inspectionLog.LocationLogId.Should().Be(locationLog.Id);
        inspectionLog.TimeOfAttempt.Should().Be(startedAt);
    }

    [Fact]
    public async Task RegisterInspectionAttemptEventHandler_WhenInspectorDoesNotExist_IgnoresEventGracefully()
    {
        var handler = new RegisterInspectionAttemptEventHandler(_dbContext);
        var evt = new InspectionStartedEvent(InspectionId.New(), InspectorId.New(), 52.0, 21.0, DateTime.UtcNow);

        await handler.Handle(evt, CancellationToken.None);

        (await _dbContext.LocationLogs.CountAsync()).Should().Be(0);
        (await _dbContext.InspectionLogs.CountAsync()).Should().Be(0);
    }
}

