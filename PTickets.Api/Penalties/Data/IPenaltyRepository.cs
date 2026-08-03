using PTickets.Api.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PTickets.Api.Penalties.Data;

public interface IPenaltyRepository
{
    Task AddAsync(Penalty penalty, CancellationToken ct = default);

    Task UpdateAsync(Penalty penalty, CancellationToken ct = default);

    Task<Penalty?> GetByIdAsync(PenaltyId id, CancellationToken ct = default);

    Task<IReadOnlyList<Penalty>> GetAllAsync(CancellationToken ct = default);
}
