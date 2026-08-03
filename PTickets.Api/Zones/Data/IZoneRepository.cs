using PTickets.Api.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PTickets.Api.Zones.Data;

public interface IZoneRepository
{
    Task AddAsync(Zone zone, CancellationToken ct = default);

    Task<Zone?> GetByIdAsync(ZoneId id, CancellationToken ct = default);

    Task<IReadOnlyList<Zone>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(ZoneId id, CancellationToken ct = default);

    Task<bool> StreetBelongsToZoneAsync(ZoneId zoneId, StreetId streetId, CancellationToken ct = default);

    /// <summary>
    /// Sprawdza, czy dla danej strefy jest płatny czas parkowania w podanym momencie.
    /// Rzuca wyjątek jeśli strefa nie istnieje.
    /// </summary>
    Task<bool> IsPaidAtDateTimeAsync(ZoneId zoneId, DateTime dateTime, CancellationToken ct = default);
}
