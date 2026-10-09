namespace PTickets.Modules.InspectorTracking.Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.InspectorTracking.Common.Data;
using PTickets.Modules.InspectorTracking.GetAllInspectors;
using PTickets.Shared;

public class GetAllInspectorsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InspectorTrackingDbContext _dbContext;

    public GetAllInspectorsTests()
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
    public async Task GetAllInspectors_WhenNoInspectors_ReturnsEmptyList()
    {
        // Act
        var result = await GetAllInspectorsEndpoint.GetAllInspectors(_dbContext, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<Ok<List<InspectorResponse>>>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllInspectors_WhenInspectorsExist_ReturnsMappedInspectors()
    {
        // Arrange
        var inspector1 = Inspector.Create("Jan", "Kowalski");
        var inspector2 = Inspector.Create("Anna", "Nowak");
        var zoneId = ZoneId.New();
        inspector2.AssignToZone(zoneId);

        await _dbContext.Inspectors.AddRangeAsync(inspector1, inspector2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await GetAllInspectorsEndpoint.GetAllInspectors(_dbContext, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<Ok<List<InspectorResponse>>>().Subject;
        okResult.Value.Should().NotBeNull();
        okResult.Value.Should().HaveCount(2);

        var item1 = okResult.Value!.First(i => i.Id == inspector1.Id.Value);
        item1.FirstName.Should().Be("Jan");
        item1.LastName.Should().Be("Kowalski");
        item1.AssignedToZone.Should().BeFalse();
        item1.ZoneId.Should().BeNull();

        var item2 = okResult.Value!.First(i => i.Id == inspector2.Id.Value);
        item2.FirstName.Should().Be("Anna");
        item2.LastName.Should().Be("Nowak");
        item2.AssignedToZone.Should().BeTrue();
        item2.ZoneId.Should().Be(zoneId.Value);
    }
}

