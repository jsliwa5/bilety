namespace PTickets.Modules.Zones.Application;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Zones.Contracts;
using PTickets.Modules.Zones.Infrastructure.Persistence;
using PTickets.Shared;

internal class ZonesModuleFacade : IZonesModule
{
    private readonly ZonesDbContext _dbContext;

    public ZonesModuleFacade(ZonesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ZoneExistsAsync(ZoneId zoneId, CancellationToken ct)
        => await _dbContext.Zones.AsNoTracking().AnyAsync(z => z.Id == zoneId, ct);

    public async Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId, CancellationToken ct)
        => await _dbContext.Streets.AsNoTracking().AnyAsync(
            s => s.Id == streetId && s.ZoneId == zoneId, ct);

    public async Task<bool> IsExcludedAsync(ZoneId zoneId, StreetId streetId, DateTime dateTime, CancellationToken ct)
    {
        var isZoneExcluded = await _dbContext.ZoneExclusions
            .AnyAsync(e => e.ZoneId == zoneId && dateTime >= e.StartDate && dateTime <= e.EndDate, ct);

        if (isZoneExcluded)
            return true;

        var isStreetExcluded = await _dbContext.StreetExclusions
            .AnyAsync(e => e.StreetId == streetId && dateTime >= e.StartDate && dateTime <= e.EndDate, ct);

        return isStreetExcluded;
    }

    public async Task<bool> IsPaidAtAsync(StreetId streetId, DateTime dateTime, CancellationToken ct)
    {
        var query = from s in _dbContext.Streets
                    join z in _dbContext.Zones on s.ZoneId equals z.Id
                    where s.Id == streetId
                    select new { StreetSchedule = s.PaidParkingSchedule, ZoneSchedule = z.PaidParkingSchedule };

        var result = await query.AsNoTracking().FirstOrDefaultAsync(ct);

        if (result is null)
            return false;

        var schedule = result.StreetSchedule ?? result.ZoneSchedule;
        return schedule?.IsPaidAt(dateTime) ?? false;
    }
}
