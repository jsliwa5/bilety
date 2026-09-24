namespace PTickets.Modules.Zones.Contracts;

using PTickets.Shared;

public interface IZonesModule
{
    Task<bool> ZoneExistsAsync(ZoneId zoneId, CancellationToken ct = default);
    Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId, CancellationToken ct = default);
    Task<bool> IsExcludedAsync(ZoneId zoneId, StreetId streetId, DateTime dateTime, CancellationToken ct = default);
    Task<bool> IsPaidAtAsync(StreetId streetId, DateTime dateTime, CancellationToken ct = default);
}
