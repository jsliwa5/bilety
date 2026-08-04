using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.Infrastructure;

public sealed class EfZoneRepository : IZoneRepository
{
    private readonly ParkingDbContext _context;

    public EfZoneRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<Zone?> GetAsync(ZoneId id)
    {
        return await _context.Zones
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Zone zone)
    {
        await _context.Zones.AddAsync(zone);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Zone zone)
    {
        _context.Zones.Update(zone);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Zone zone)
    {
        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(ZoneId id)
    {
        return await _context.Zones
            .AnyAsync(x => x.Id == id);
    }
}