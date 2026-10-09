using MediatR;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Modules.Notices.Common.Data;

namespace PTickets.Modules.Notices.IssueNotice;

public class NoticeIssuedEventHandler : INotificationHandler<NoticeIssuedEvent>
{
    private readonly NoticesDbContext _dbContext;

    public NoticeIssuedEventHandler(NoticesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(NoticeIssuedEvent notification, CancellationToken cancellationToken)
    {
        var notice = Notice.Create(
            notification.NoticeId,
            notification.InspectionId,
            notification.RegistrationNumber,
            notification.PenaltyAmount,
            notification.Surcharge,
            notification.IssuedAt
        );

        await _dbContext.Notices.AddAsync(notice, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

