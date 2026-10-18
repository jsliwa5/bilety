using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using PTickets.Modules.Zones.Contracts.Events;
using PTickets.Modules.Zones.Common.Data;
using PTickets.Modules.Zones.Common.Exceptions;

namespace PTickets.Modules.Zones.CreateStreet;

public static class CreateStreetEndpoint
{
    public static void MapCreateStreetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/zones/{zoneId:guid}/streets", HandleAsync);
    }

    internal static async Task<IResult> HandleAsync(
        Guid zoneId,
        CreateStreetRequest request,
        ZonesDbContext dbContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var zone = await dbContext.Zones
            .Include(z => z.Streets)
            .FirstOrDefaultAsync(z => z.Id == new ZoneId(zoneId), ct);

        if (zone == null)
        {
            throw new ZoneNotFoundException();
        }

        PaidParkingSchedule? schedule = null;

        bool hasAnyScheduleField = request.StartTime.HasValue || request.EndTime.HasValue || request.PaidDays != null;
        bool hasAllScheduleFields = request.StartTime.HasValue && request.EndTime.HasValue && request.PaidDays != null;

        if (hasAnyScheduleField && !hasAllScheduleFields)
        {
            throw new InvalidParkingScheduleException("Aby nadpisać harmonogram ulicy, należy podać wszystkie pola: StartTime, EndTime oraz PaidDays.");
        }

        if (hasAllScheduleFields)
        {
            schedule = new PaidParkingSchedule(
                request.StartTime!.Value,
                request.EndTime!.Value,
                request.PaidDays
            );
        }

        var newStreet = zone.AddStreet(request.Name, schedule);

        await dbContext.SaveChangesAsync(ct);

        await mediator.Publish(new StreetCreatedEvent(newStreet.Id, newStreet.ZoneId, newStreet.Name), ct);

        return Results.Created($"/api/zones/{zoneId}/streets/{newStreet.Id.Value}", new { Id = newStreet.Id.Value });
    }
}

