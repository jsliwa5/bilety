using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using PTickets.Modules.Zones.Common.Data;
using Microsoft.AspNetCore.Http;
using PTickets.Modules.Zones.Common.Exceptions;

namespace PTickets.Modules.Zones.CreateZoneExclusion;

public static class CreateZoneExclusionEndpoint
{
    public static void MapCreateZoneExclusionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/zones/{zoneId:guid}/exclusions", async (Guid zoneId, CreateZoneExclusionRequest request, ZonesDbContext dbContext, CancellationToken ct) =>
        {
            var zone = await dbContext.Zones
                .Include(z => z.Exclusions)
                .FirstOrDefaultAsync(z => z.Id == new ZoneId(zoneId), ct);

            if (zone is null)
                throw new ZoneNotFoundException();

            var startDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);
            var newExclusion = zone.AddExclusion(
                startDate,
                endDate,
                request.Reason
            );

            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/zones/{zoneId}/exclusions/{newExclusion.Id}", newExclusion);

        });
    }
}

