using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Contracts;
using PTickets.Shared;

namespace PTickets.Modules.InspectorTracking.AssignToZone;

public record AssignToZoneRequest(Guid ZoneId);

public static class AssignToZoneEndpoint
{
    public static void MapAssignToZone(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/inspectors/{inspectorId:guid}/zone", AssignToZone)
            .WithName("AssignInspectorToZone")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/api/inspectors/{inspectorId:guid}/zone", UnassignFromZone)
            .WithName("UnassignInspectorFromZone")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AssignToZone(
        Guid inspectorId,
        AssignToZoneRequest request,
        InspectorTrackingDbContext dbContext,
        IZonesModule zonesModule,
        CancellationToken ct)
    {
        var inspector = await dbContext.Inspectors
            .FirstOrDefaultAsync(i => i.Id == new InspectorId(inspectorId), ct);

        if (inspector is null)
            return Results.NotFound("Inspektor nie został znaleziony.");

        var zoneId = new ZoneId(request.ZoneId);
        var zoneExists = await zonesModule.ZoneExistsAsync(zoneId, ct);
        if (!zoneExists)
            return Results.BadRequest("Podana strefa nie istnieje.");

        inspector.AssignToZone(zoneId);
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<IResult> UnassignFromZone(
        Guid inspectorId,
        InspectorTrackingDbContext dbContext,
        CancellationToken ct)
    {
        var inspector = await dbContext.Inspectors
            .FirstOrDefaultAsync(i => i.Id == new InspectorId(inspectorId), ct);

        if (inspector is null)
            return Results.NotFound("Inspektor nie został znaleziony.");

        inspector.UnassignFromZone();
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}

