using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.Data;

namespace PTickets.Modules.Zones.CreateStreet;

public static class CreateStreetEndpoint
{
    public static void MapCreateStreetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/zones/{zoneId:guid}/streets", async (Guid zoneId, CreateStreetRequest request, ZonesDbContext dbContext, IMediator mediator, CancellationToken ct) =>
        {
            var zone = await dbContext.Zones
                .Include(z => z.Streets)
                .FirstOrDefaultAsync(z => z.Id == new ZoneId(zoneId), ct);

            if (zone == null)
            {
                return Results.BadRequest("Zone not found");
            }

            PaidParkingSchedule? schedule = null;

            if (request.StartTime.HasValue && request.EndTime.HasValue && request.PaidDays != null)
            {
                schedule = new PaidParkingSchedule(
                    request.StartTime.Value,
                    request.EndTime.Value,
                    request.PaidDays
                );
            }

            Street newStreet;
            try
            {
                newStreet = zone.AddStreet(request.Name, schedule);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }

            await dbContext.SaveChangesAsync(ct);

            await mediator.Publish(new StreetCreatedEvent(newStreet.Id, newStreet.ZoneId, newStreet.Name), ct);

            return Results.Created($"/api/zones/{zoneId}/streets/{newStreet.Id.Value}", new { Id = newStreet.Id.Value });
        });
    }
}
