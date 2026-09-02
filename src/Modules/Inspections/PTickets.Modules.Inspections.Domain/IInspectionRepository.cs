namespace PTickets.Modules.Inspections.Domain;

using PTickets.Shared;

public interface IInspectionRepository
{
    Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken cancellationToken = default);
    Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default);
}

