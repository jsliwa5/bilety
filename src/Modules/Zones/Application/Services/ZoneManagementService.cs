namespace PTickets.Modules.Zones.Application.Services;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Application.Dtos;
using PTickets.Modules.Zones.Domain;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared;
using PTickets.Modules.Zones.Contracts.Events;

public class ZoneManagementService(ZonesDbContext dbContext, IMediator mediator)
{
    public async Task<ZoneId> CreateZoneAsync(
        string name, 
        ZoneType type, 
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        DayOfWeek[]? paidDays = null,
        CancellationToken cancellationToken = default)
    {
        PaidParkingSchedule? schedule = null;
        if (startTime.HasValue && endTime.HasValue)
        {
            schedule = new PaidParkingSchedule(startTime.Value, endTime.Value, paidDays ?? []);
        }

        var zone = Zone.Create(name, type, schedule);
        dbContext.Zones.Add(zone);
        
        Street? autoStreet = null;
        if (type == ZoneType.Single)
        {
            autoStreet = Street.CreateZoneRepresentative(zone.Id, name);
            dbContext.Streets.Add(autoStreet);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new ZoneCreatedEvent(zone.Id, zone.Name), cancellationToken);

        if (autoStreet != null)
        {
            await mediator.Publish(new StreetCreatedEvent(autoStreet.Id, zone.Id, autoStreet.Name), cancellationToken);
        }

        return zone.Id;
    }

    public async Task<StreetId> CreateStreetAsync(
        ZoneId zoneId,
        string name,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        DayOfWeek[]? paidDays = null,
        CancellationToken cancellationToken = default)
    {
        var zone = await dbContext.Zones.FirstOrDefaultAsync(z => z.Id == zoneId, cancellationToken);
        if (zone == null)
            throw new InvalidOperationException($"Strefa o ID {zoneId} nie istnieje.");
            
        if (zone.Type == ZoneType.Single)
            throw new InvalidOperationException($"Nie można dodawać nowych ulic do pojedynczej strefy (Single).");

        PaidParkingSchedule? schedule = null;
        if (startTime.HasValue && endTime.HasValue)
        {
            schedule = new PaidParkingSchedule(startTime.Value, endTime.Value, paidDays ?? []);
        }

        var street = Street.CreateNormalStreet(zoneId, name, schedule);
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
            z.Type,
            z.PaidParkingSchedule != null ? new ScheduleResponse(
                z.PaidParkingSchedule.StartTime,
                z.PaidParkingSchedule.EndTime,
                z.PaidParkingSchedule.PaidDays
            ) : null,
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
