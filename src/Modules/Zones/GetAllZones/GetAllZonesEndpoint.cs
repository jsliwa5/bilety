using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Data;

namespace PTickets.Modules.Zones.GetAllZones;

public static class GetAllZonesEndpoint
{
    public static void MapGetAllZonesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/zones", async (ZonesDbContext dbContext, CancellationToken ct) =>
        {
            var zones = await dbContext.Zones
                .Include(z => z.Streets)
                .AsNoTracking()
                .ToListAsync(ct);

            var results = zones.Select(
                zone => MapToZoneResponse(zone))
            ;
            return Results.Ok(results);
        });
    }

    private static ScheduleResponse MapToScheduleResponse(PaidParkingSchedule schedule)
    {
        return new ScheduleResponse(
            schedule.StartTime,
            schedule.EndTime,
            schedule.PaidDays
        );
    }

    private static StreetResponse MapToStreetResponse(Street street)
    {
        return new StreetResponse(
            street.Id.Value,
            street.Name,
            street.RepresentsWholeZone,
            street.PaidParkingSchedule != null ? MapToScheduleResponse(street.PaidParkingSchedule) : null
        );
    }

    private static ZoneResponse MapToZoneResponse(Zone zone)
    {
        return new ZoneResponse(
            zone.Id.Value,
            zone.Name,
            zone.Type,
            zone.PaidParkingSchedule != null ? MapToScheduleResponse(zone.PaidParkingSchedule) : null,
            zone.Streets.Select(MapToStreetResponse).ToList()
        );
    }
}
