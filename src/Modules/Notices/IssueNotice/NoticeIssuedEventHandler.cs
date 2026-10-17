using System.Text.Json;
using MediatR;
using PTickets.Modules.Inspections.Contracts.Events;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.ExternalSync;

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

        var exportItem = new ExternalNoticeItemDto(
            notice.Id.Value,
            notice.InspectionId.Value,
            notice.RegistrationNumber.Value,
            notice.PenaltyAmount,
            notice.Surcharge,
            notice.TotalAmount,
            notice.Status.ToString(),
            notice.IssuedAt);

        var outboxMessage = NoticeOutboxMessage.Create(
            notice.Id.Value,
            "NoticeCreatedForDailyExport",
            JsonSerializer.Serialize(exportItem),
            DateTime.UtcNow);

        await _dbContext.Notices.AddAsync(notice, cancellationToken);
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

