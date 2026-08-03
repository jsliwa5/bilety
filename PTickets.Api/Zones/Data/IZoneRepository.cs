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
}
