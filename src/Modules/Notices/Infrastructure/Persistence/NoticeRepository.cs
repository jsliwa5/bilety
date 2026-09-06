using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notices.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Notices.Infrastructure.Persistence;

public class NoticeRepository : INoticeRepository
{
    private readonly NoticesDbContext _dbContext;

    public NoticeRepository(NoticesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Notice?> GetByIdAsync(NoticeId id, CancellationToken ct)
    {
        return await _dbContext.Notices
            
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

