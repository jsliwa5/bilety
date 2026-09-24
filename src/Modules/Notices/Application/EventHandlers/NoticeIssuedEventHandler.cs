using MediatR;
using PTickets.Modules.Notices.Domain;
using PTickets.Modules.Inspections.Contracts.Events;

namespace PTickets.Modules.Notices.Application.EventHandlers;

public class NoticeIssuedEventHandler : INotificationHandler<NoticeIssuedEvent>
{
    private readonly INoticeRepository _noticeRepository;

    public NoticeIssuedEventHandler(INoticeRepository noticeRepository)
    {
        _noticeRepository = noticeRepository;
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

        await _noticeRepository.AddAsync(notice, cancellationToken);
        await _noticeRepository.SaveChangesAsync(cancellationToken);
    }
}
