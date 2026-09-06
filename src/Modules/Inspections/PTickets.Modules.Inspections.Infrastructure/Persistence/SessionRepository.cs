using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

public class SessionRepository : ISessionRepository
{
    private readonly InspectionsDbContext _dbContext;

    public SessionRepository(InspectionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Session?> GetByIdAsync(SessionId id, CancellationToken ct)
    {
        return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Session?> GetOpenSessionForInspectorAsync(InspectorId id, CancellationToken ct)
    {
        return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.InspectorId == id && s.ClosedAt == null, ct);
    }

    public async Task AddAsync(Session session, CancellationToken ct)
    {
        await _dbContext.Sessions.AddAsync(session, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}

