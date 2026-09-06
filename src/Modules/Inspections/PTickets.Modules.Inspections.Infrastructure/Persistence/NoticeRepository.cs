using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

public class NoticeRepository : INoticeRepository
{
    private readonly InspectionsDbContext _dbContext;

    public NoticeRepository(InspectionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notice?> GetByIdAsync(NoticeId id, CancellationToken ct)
    {
        return await _dbContext.Notices
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task AddAsync(Notice notice, CancellationToken ct)
    {
        await _dbContext.Notices.AddAsync(notice, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}

