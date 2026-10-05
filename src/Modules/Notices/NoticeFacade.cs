using PTickets.Modules.Notices.Contracts;
using PTickets.Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace PTickets.Modules.Notices;

public class NoticeFacade : INoticesModule
{
    private readonly NoticesDbContext _dbContext;

    public NoticeFacade(NoticesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> WasNoticeIssuedForDateAsync(RegistrationNumber registrationNumber, DateTime date, CancellationToken cancellationToken)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);

        return await _dbContext.Notices.AnyAsync(
            n => n.RegistrationNumber == registrationNumber 
                 && n.IssuedAt >= day 
                 && n.IssuedAt < nextDay, 
            cancellationToken);
    }
}

