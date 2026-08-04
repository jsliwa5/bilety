using PTickets.Api.Common;

namespace PTickets.Api.Zones.Data;

public interface IStreetRepository
{
    Task<Street?> GetAsync(StreetId id);

    Task<IReadOnlyList<Street>> GetByZoneAsync(
        ZoneId zoneId);

    Task AddAsync(Street street);

    Task UpdateAsync(Street street);

    Task DeleteAsync(Street street);

    Task<bool> ExistsAsync(StreetId id);
}
