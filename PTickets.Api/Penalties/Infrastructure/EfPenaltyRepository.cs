using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Penalties.Data;


namespace PTickets.Api.Penalties.Infrastructure;

public class EfPenaltyRepository : IPenaltyRepository
{
    private readonly ParkingDbContext _context;

    public EfPenaltyRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Penalty penalty, CancellationToken ct = default)
    {
        await _context.Penalties.AddAsync(penalty, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Penalty penalty, CancellationToken ct = default)
    {
        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Penalty?> GetByIdAsync(PenaltyId id, CancellationToken ct = default)
    {
        return await _context.Penalties
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Penalty>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Penalties
            .AsNoTracking()
            .ToListAsync(ct);
    }
}