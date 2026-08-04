using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.Infrastructure;

public sealed class EfStreetRepository : IStreetRepository
{
    private readonly ParkingDbContext _context;

    public EfStreetRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<Street?> GetAsync(StreetId id)
    {
        return await _context.Streets
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Street>> GetByZoneAsync(ZoneId zoneId)
    {
        return await _context.Streets
            .Where(x => x.ZoneId == zoneId)
            .ToListAsync();
    }

    public async Task AddAsync(Street street)
    {
        await _context.Streets.AddAsync(street);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Street street)
    {
        _context.Streets.Update(street);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Street street)
    {
        _context.Streets.Remove(street);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(StreetId id)
    {
        return await _context.Streets.AnyAsync(x => x.Id == id);
    }
}