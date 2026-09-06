using PTickets.Shared;

namespace PTickets.Modules.Inspections.Domain;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(SessionId id, CancellationToken ct);
    Task<Session?> GetOpenSessionForInspectorAsync(InspectorId id, CancellationToken ct);
    Task AddAsync(Session session, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

