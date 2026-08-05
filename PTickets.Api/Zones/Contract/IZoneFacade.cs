using PTickets.Api.Common;

namespace PTickets.Api.Zones.Contract;

public interface IZoneFacade
{
    Task<bool> ExistsByIdAsync(ZoneId zoneId);
    Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId);

    /// <summary>
    /// Sprawdza, czy dla danej strefy jest płatny czas parkowania w podanym momencie.
    /// Rzuca wyjątek jeśli strefa nie istnieje.
    /// </summary>
    Task<bool> IsPaidAtDateTimeAsync(StreetId streetId, DateTime dateTime);

    Task<decimal> CalculatePenaltyAmountAsync(StreetId streetid, DateTime inspectionDate);
}
