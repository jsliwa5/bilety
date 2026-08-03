using PTickets.Api.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PTickets.Api.Inspections.Data;

public interface IInspectionRepository
{
    Task AddAsync(Inspection inspection, CancellationToken ct = default);

    Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken ct = default);

    Task<IReadOnlyList<Inspection>> GetAllAsync(CancellationToken ct = default);
}
