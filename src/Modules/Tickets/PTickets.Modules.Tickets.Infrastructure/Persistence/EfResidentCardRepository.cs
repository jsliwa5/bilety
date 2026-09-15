namespace PTickets.Modules.Tickets.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class EfResidentCardRepository(TicketsDbContext dbContext) : IResidentCardRepository
{
    public async Task<ResidentCard?> FindActiveCardAsync(RegistrationNumber registration, StreetId streetId, DateTime at, CancellationToken ct = default)
    {
        return await dbContext.ResidentCards
            .Where(c => c.RegistrationNumber == registration && c.StreetId == streetId && c.ValidFrom <= at && c.ValidTo >= at)
            .OrderByDescending(c => c.ValidTo)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(ResidentCard residentCard, CancellationToken ct = default)
    {
        await dbContext.ResidentCards.AddAsync(residentCard, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}

