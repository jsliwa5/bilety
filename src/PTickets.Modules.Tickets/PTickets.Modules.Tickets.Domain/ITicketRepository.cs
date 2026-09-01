namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public interface ITicketRepository
{
    Task<Ticket?> FindActiveTicketAsync(RegistrationNumber reg, StreetId streetId, DateTime at, CancellationToken ct = default);
    Task AddAsync(Ticket ticket, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

