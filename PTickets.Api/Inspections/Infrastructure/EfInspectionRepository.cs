using PTickets.Api.Inspections.Data;
using PTickets.Api.Database ;
using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;

namespace PTickets.Api.Inspections.Infrastructure;

public class EfInspectionRepository : IInspectionRepository
{
    private readonly ParkingDbContext _context;

    public EfInspectionRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Inspection inspection, CancellationToken ct = default)
    {
        await _context.Inspections.AddAsync(inspection, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken ct = default)
    {
        return await _context.Inspections
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Inspection>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Inspections
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
