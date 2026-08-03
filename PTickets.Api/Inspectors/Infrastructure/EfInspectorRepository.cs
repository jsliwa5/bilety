using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Inspectors.Data;


namespace PTickets.Api.Inspectors.Infrastructure;

public class EfInspectorRepository : IInspectorRepository
{
    private readonly ParkingDbContext _context;

    public EfInspectorRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Inspector inspector, CancellationToken ct = default)
    {
        await _context.Inspectors.AddAsync(inspector, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Inspector inspector, CancellationToken ct = default)
    {
        _context.Inspectors.Update(inspector);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Inspector?> GetByIdAsync(InspectorId id, CancellationToken ct = default)
    {
        return await _context.Inspectors
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Inspector>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Inspectors
            .AsNoTracking()
            .ToListAsync(ct);
    }
}