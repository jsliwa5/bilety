namespace PTickets.Modules.Tickets.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;

public class EfStreetZoneMappingRepository(TicketsDbContext dbContext) : IStreetZoneMappingRepository
{
    public async Task<StreetZoneMapping?> GetByStreetIdAsync(StreetId streetId, CancellationToken ct = default)
    {
        return await dbContext.StreetZoneMappings
            .FirstOrDefaultAsync(m => m.StreetId == streetId, ct);
    }

    public async Task AddAsync(StreetZoneMapping mapping, CancellationToken ct = default)
    {
        await dbContext.StreetZoneMappings.AddAsync(mapping, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}

