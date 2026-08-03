using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Zones.Data;


namespace PTickets.Api.Zones.Infrastructure;

public class EfZoneRepository : IZoneRepository
{
    private readonly ParkingDbContext _context;

    public EfZoneRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Zone zone, CancellationToken ct = default)
    {
        await _context.Zones.AddAsync(zone, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Zone?> GetByIdAsync(ZoneId id, CancellationToken ct = default)
    {
        // Pociągamy ulice razem ze strefą, ponieważ są częścią tego samego Agregatu
        return await _context.Zones
            .Include(z => z.Streets)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Zone>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Zones
            .Include(z => z.Streets)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByIdAsync(ZoneId id, CancellationToken ct = default)
    {
        return await _context.Zones.AnyAsync(x => x.Id == id, ct);
    }

    public async Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId, CancellationToken ct = default)
    {
        return await _context.Zones
            .Where(z => z.Id == zoneId)
            .SelectMany(z => z.Streets)
            .AnyAsync(s => s.Id == streetId, ct);
    }

    public async Task<bool> IsPaidAtDateTimeAsync(ZoneId zoneId, DateTime dateTime, CancellationToken ct = default)
    {
        var zone = await _context.Zones
            .Where(z => z.Id == zoneId)
            .Select(z => new { z.Id, z.PaidParkingSchedule })
            .FirstOrDefaultAsync(ct);

        if (zone == null)
            throw new InvalidOperationException($"Strefa o ID {zoneId} nie istnieje.");

        return zone.PaidParkingSchedule?.IsPaidTime(dateTime) ?? false;
    }
}