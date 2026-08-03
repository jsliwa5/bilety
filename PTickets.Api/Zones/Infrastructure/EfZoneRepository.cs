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

}