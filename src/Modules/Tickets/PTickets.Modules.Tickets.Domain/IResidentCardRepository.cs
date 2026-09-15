namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public interface IResidentCardRepository
{
    Task<ResidentCard?> FindActiveCardAsync(RegistrationNumber registration, StreetId streetId, DateTime at, CancellationToken ct = default);
    Task AddAsync(ResidentCard residentCard, CancellationToken ct = default);
}
