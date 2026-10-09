using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using Microsoft.AspNetCore.Http;
using PTickets.Modules.Zones.Common.Data;
using PTickets.Modules.Zones.Common.Exceptions;

namespace PTickets.Modules.Zones.CreateStreetExclusion;

public static class CreateStreetExclusionEndpoint
{
    public static void MapCreateStreetExclusionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/streets/{streetId:guid}/exclusions", async (Guid streetId, CreateStreetExclusionRequest request, ZonesDbContext dbContext, CancellationToken ct) =>
        {
            var streetExists = await dbContext.Streets.AnyAsync(s => s.Id == new StreetId(streetId), ct);
            if (!streetExists)
                throw new StreetNotFoundException();

            var newStreetExclusion = StreetExclusion.Create(
                    new StreetId(streetId),
                    request.StartDate,
                    request.EndDate,
                    request.Reason
                );

            await dbContext.StreetExclusions.AddAsync(newStreetExclusion, ct);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/streets/{streetId}/exclusions/{newStreetExclusion.Id}", new { Id = newStreetExclusion.Id });

        });
    }
}

