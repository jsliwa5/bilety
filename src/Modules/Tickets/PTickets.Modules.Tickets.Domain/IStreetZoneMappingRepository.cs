namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;

public interface IStreetZoneMappingRepository
{
    Task<StreetZoneMapping?> GetByStreetIdAsync(StreetId streetId, CancellationToken ct = default);
    Task AddAsync(StreetZoneMapping mapping, CancellationToken ct = default);
}

