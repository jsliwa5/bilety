namespace PTickets.Modules.Zones.Application.Services;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Application.Dtos;
using PTickets.Modules.Zones.Domain;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared;
using PTickets.Shared.Contracts.Zones;

public class ZoneManagementService(ZonesDbContext dbContext, IMediator mediator)
{
    public async Task<ZoneId> CreateZoneAsync(string name, CancellationToken cancellationToken = default)
    {
        var zone = Zone.Create(name);
        dbContext.Zones.Add(zone);
        await dbContext.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ZoneCreatedEvent(zone.Id, zone.Name), cancellationToken);

        return zone.Id;
    }

    public async Task<StreetId> CreateStreetAsync(
        ZoneId zoneId,
        string name,
        bool representsWholeZone = false,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        DayOfWeek[]? paidDays = null,
        CancellationToken cancellationToken = default)
    {
        var zoneExists = await dbContext.Zones.AnyAsync(z => z.Id == zoneId, cancellationToken);
        if (!zoneExists)
            throw new InvalidOperationException($"Strefa o ID {zoneId} nie istnieje.");

        PaidParkingSchedule? schedule = null;
        if (startTime.HasValue && endTime.HasValue)
        {
            schedule = new PaidParkingSchedule(startTime.Value, endTime.Value, paidDays ?? []);
        }

        var street = Street.Create(zoneId, name, representsWholeZone, schedule);
        dbContext.Streets.Add(street);
        await dbContext.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new StreetCreatedEvent(street.Id, zoneId, street.Name), cancellationToken);

        return street.Id;
    }

    public async Task<Guid> AddZoneExclusionAsync(
        ZoneId zoneId,
        DateTime start,
        DateTime end,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var zoneExists = await dbContext.Zones.AnyAsync(z => z.Id == zoneId, cancellationToken);
        if (!zoneExists)
            throw new InvalidOperationException($"Strefa o ID {zoneId} nie istnieje.");

        var exclusion = ZoneExclusion.Create(zoneId, start, end, reason);
        dbContext.ZoneExclusions.Add(exclusion);
        await dbContext.SaveChangesAsync(cancellationToken);

        return exclusion.Id;
    }

    public async Task<Guid> AddStreetExclusionAsync(
        StreetId streetId,
        DateTime start,
        DateTime end,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var streetExists = await dbContext.Streets.AnyAsync(s => s.Id == streetId, cancellationToken);
        if (!streetExists)
            throw new InvalidOperationException($"Ulica o ID {streetId} nie istnieje.");

        var exclusion = StreetExclusion.Create(streetId, start, end, reason);
        dbContext.StreetExclusions.Add(exclusion);
        await dbContext.SaveChangesAsync(cancellationToken);

        return exclusion.Id;
    }

    public async Task<IReadOnlyList<ZoneResponse>> GetAllZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await dbContext.Zones
            .Include(z => z.Streets)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return zones.Select(z => new ZoneResponse(
            z.Id.Value,
            z.Name,
            z.Streets.Select(s => new StreetResponse(
                s.Id.Value,
                s.Name,
                s.RepresentsWholeZone,
                s.PaidParkingSchedule != null ? new ScheduleResponse(
                    s.PaidParkingSchedule.StartTime,
                    s.PaidParkingSchedule.EndTime,
                    s.PaidParkingSchedule.PaidDays
                ) : null
            )).ToList()
        )).ToList();
    }
}
