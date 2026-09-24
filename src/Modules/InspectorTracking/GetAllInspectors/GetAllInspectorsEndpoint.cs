namespace PTickets.Modules.InspectorTracking.GetAllInspectors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class GetAllInspectorsEndpoint
{
    public static void MapGetAllInspectors(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inspectors", GetAllInspectors)
            .WithName("GetAllInspectors")
            .Produces<IReadOnlyList<InspectorResponse>>(StatusCodes.Status200OK);
    }

    internal static async Task<IResult> GetAllInspectors(
        InspectorTrackingDbContext dbContext,
        CancellationToken ct)
    {
        var inspectors = await dbContext.Inspectors
            .AsNoTracking()
            .ToListAsync(ct);

        var response = inspectors
            .Select(i => new InspectorResponse(
                i.Id.Value,
                i.FirstName ?? string.Empty,
                i.LastName ?? string.Empty,
                i.AssignedToZone,
                i.ZoneId?.Value))
            .ToList();

        return Results.Ok(response);
    }
}

