using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.AddZone;

public static class AddZoneEndpoint
{
    public static void MapAddZone(this WebApplication app)
    {
        app.MapPost("/zones", AddZone)
        .WithName("AddZone")
        .Produces(StatusCodes.Status200OK);
    }

    private static async Task<AddZoneResponse> AddZone(
        AddZoneRequest request, 
        IZoneRepository zoneRepository)
    {
        
        var zone = Zone.Create(request.Name);

        await zoneRepository.AddAsync(zone);

        return new AddZoneResponse(zone.Id);
    }
}
