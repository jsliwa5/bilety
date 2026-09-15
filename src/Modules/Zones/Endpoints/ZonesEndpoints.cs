namespace PTickets.Modules.Zones.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Zones.Application.Dtos;
using PTickets.Modules.Zones.Application.Services;
using PTickets.Shared;

public static class ZonesEndpoints
{
    public static IEndpointRouteBuilder MapZonesApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/zones");

        group.MapPost("", async (CreateZoneRequest request, ZoneManagementService service, CancellationToken ct) =>
        {
            if (!Enum.IsDefined(typeof(PTickets.Modules.Zones.Domain.ZoneType), request.Type))
            {
                return Results.BadRequest(new { Error = $"Nieprawidłowy typ strefy: {request.Type}" });
            }

            var zoneId = await service.CreateZoneAsync(
                request.Name, 
                request.Type, 
                request.StartTime, 
                request.EndTime, 
                request.PaidDays, 
                ct);
            return Results.Created($"/api/zones/{zoneId.Value}", new { Id = zoneId.Value });
        });

        group.MapGet("", async (ZoneManagementService service, CancellationToken ct) =>
        {
            var zones = await service.GetAllZonesAsync(ct);
            return Results.Ok(zones);
        });

        group.MapPost("{zoneId:guid}/streets", async (Guid zoneId, CreateStreetRequest request, ZoneManagementService service, CancellationToken ct) =>
        {
            var streetId = await service.CreateStreetAsync(
                new ZoneId(zoneId),
                request.Name,
                request.StartTime,
                request.EndTime,
                request.PaidDays,
                ct);

            return Results.Created($"/api/zones/{zoneId}/streets/{streetId.Value}", new { Id = streetId.Value });
        });

        return endpoints;
    }
}
