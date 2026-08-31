namespace PTickets.Modules.Zones.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Zones.Application.Dtos;
using PTickets.Modules.Zones.Application.Services;
using PTickets.Shared;

public static class ExclusionsEndpoints
{
    public static IEndpointRouteBuilder MapExclusionsApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/zones/{zoneId:guid}/exclusions", async (Guid zoneId, CreateExclusionRequest request, ZoneManagementService service, CancellationToken ct) =>
        {
            var exclusionId = await service.AddZoneExclusionAsync(
                new ZoneId(zoneId),
                request.StartDate,
                request.EndDate,
                request.Reason,
                ct);

            return Results.Created($"/api/zones/{zoneId}/exclusions/{exclusionId}", new { Id = exclusionId });
        });

        endpoints.MapPost("/api/streets/{streetId:guid}/exclusions", async (Guid streetId, CreateExclusionRequest request, ZoneManagementService service, CancellationToken ct) =>
        {
            var exclusionId = await service.AddStreetExclusionAsync(
                new StreetId(streetId),
                request.StartDate,
                request.EndDate,
                request.Reason,
                ct);

            return Results.Created($"/api/streets/{streetId}/exclusions/{exclusionId}", new { Id = exclusionId });
        });

        return endpoints;
    }
}
