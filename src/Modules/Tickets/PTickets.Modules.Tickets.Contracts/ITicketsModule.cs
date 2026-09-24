namespace PTickets.Modules.Tickets.Contracts;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public interface ITicketsModule
{
    Task<TicketCheckResult> CheckRegistrationAsync(
        RegistrationNumber registration,
        StreetId streetId,
        DateTime at,
        CancellationToken ct = default);
}
