using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.Data;

namespace PTickets.Modules.Zones.CreateZone;

public static class CreateZoneEndpoint
{
    public static void MapCreateZoneEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/zones", async (CreateZoneRequest request, ZonesDbContext dbContext, IMediator mediator, CancellationToken ct) =>
        {
            if (!Enum.IsDefined(typeof(ZoneType), request.Type))
            {
                return Results.BadRequest(new { Error = $"Nieprawidłowy typ strefy: {request.Type}" });
            }

            var newZone = Zone.Create(
                request.Name,
                request.Type,
                new PaidParkingSchedule(request.StartTime, request.EndTime, request.PaidDays)
            );

            await dbContext.Zones.AddAsync(newZone, ct);
            await dbContext.SaveChangesAsync(ct);

            await mediator.Publish(new ZoneCreatedEvent(newZone.Id, newZone.Name), ct);

            foreach (var street in newZone.Streets)
            {
                await mediator.Publish(new StreetCreatedEvent(street.Id, newZone.Id, street.Name), ct);
            }

            return Results.Created($"/api/zones/{newZone.Id.Value}", new { Id = newZone.Id.Value });
        });
    }



}
