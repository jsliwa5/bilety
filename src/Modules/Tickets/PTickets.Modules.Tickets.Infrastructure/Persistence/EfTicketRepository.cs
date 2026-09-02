namespace PTickets.Modules.Tickets.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class EfTicketRepository(TicketsDbContext dbContext) : ITicketRepository
{
    public async Task<Ticket?> FindActiveTicketAsync(
        RegistrationNumber reg,
        StreetId streetId,
        DateTime at,
        CancellationToken ct = default)
    {
        return await dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.RegistrationNumber == reg &&
                t.StreetId == streetId &&
                t.ValidFrom <= at &&
                t.ValidTo >= at,
                ct);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await dbContext.Tickets.AddAsync(ticket, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}

