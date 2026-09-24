using PTickets.Shared;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Domain;

public interface IInspectionRepository
{
    Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken ct);
    Task AddAsync(Inspection inspection, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<bool> HasInspectionForVehicleTodayAsync(RegistrationNumber registrationNumber, CancellationToken ct);
}

