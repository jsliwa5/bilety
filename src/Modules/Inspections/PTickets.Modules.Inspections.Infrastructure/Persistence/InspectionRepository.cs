using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

public class InspectionRepository : IInspectionRepository
{
    private readonly InspectionsDbContext _dbContext;

    public InspectionRepository(InspectionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken ct)
    {
        return await _dbContext.Inspections
            .Include(i => i.Violations)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task AddAsync(Inspection inspection, CancellationToken ct)
    {
        await _dbContext.Inspections.AddAsync(inspection, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        // Debug: log all entity states
        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            Console.WriteLine($"[EF DEBUG] Entity: {entry.Entity.GetType().Name}, State: {entry.State}, Key: {string.Join(",", entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue))}");
        }
        await _dbContext.SaveChangesAsync(ct);
    }
}

