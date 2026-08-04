using PTickets.Api.Common;


namespace PTickets.Api.Zones.Data;

public interface IZoneRepository
{
    Task<Zone?> GetAsync(ZoneId id);

    Task AddAsync(Zone zone);

    Task UpdateAsync(Zone zone);

    Task DeleteAsync(Zone zone);

    Task<bool> ExistsAsync(ZoneId id);
}
