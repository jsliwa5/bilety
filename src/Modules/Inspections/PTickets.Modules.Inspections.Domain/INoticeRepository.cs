using PTickets.Shared;

namespace PTickets.Modules.Inspections.Domain;

public interface INoticeRepository
{
    Task<Notice?> GetByIdAsync(NoticeId id, CancellationToken ct);
    Task AddAsync(Notice notice, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

