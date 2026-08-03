using PTickets.Api.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PTickets.Api.Inspectors.Data;

public interface IInspectorRepository
{
    Task AddAsync(Inspector inspector, CancellationToken ct = default);

    Task UpdateAsync(Inspector inspector, CancellationToken ct = default);

    Task<Inspector?> GetByIdAsync(InspectorId id, CancellationToken ct = default);

    Task<IReadOnlyList<Inspector>> GetAllAsync(CancellationToken ct = default);
}
