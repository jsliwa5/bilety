namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public interface ITicketProvider
{
    string ProviderName { get; }
    Task<TicketCheckResult> CheckAsync(RegistrationNumber registration, StreetId streetId, DateTime at, CancellationToken ct = default);
}

