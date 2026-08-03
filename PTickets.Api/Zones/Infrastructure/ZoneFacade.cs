using PTickets.Api.Common;
using PTickets.Api.Zones.Contract;
using PTickets.Api.Zones.Data;

namespace PTickets.Api.Zones.Infrastructure;

public class ZoneFacade : IZoneFacade
{

    private readonly IZoneRepository _zoneRepository;

    public ZoneFacade(IZoneRepository zoneRepository)
    {
        _zoneRepository = zoneRepository;
    }

    public async Task<bool> ExistsByIdAsync(ZoneId zoneId)
    {
        return await _zoneRepository.ExistsByIdAsync(zoneId);
    }

    public async Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId)
    {
        return await _zoneRepository.StreetBelongsToZoneAsync(zoneId, streetId);
    }

    public async Task<bool> IsPaidAtDateTimeAsync(ZoneId zoneId, DateTime dateTime)
    {
        return await _zoneRepository.IsPaidAtDateTimeAsync(zoneId, dateTime);
    }
}
