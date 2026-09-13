namespace PTickets.Modules.Notifications.Application.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notifications.Domain;
using PTickets.Modules.Notifications.Infrastructure.External;
using PTickets.Modules.Notifications.Infrastructure.Persistence;
using PTickets.Shared.Abstractions;
using PTickets.Shared.Contracts.Inspections;
using PTickets.Shared.Contracts.Notifications;

public class NoticeIssuedSendSmsHandler(
    NotificationsDbContext dbContext,
    ISmsGateway smsGateway,
    IPublisher publisher,
    IDateTimeProvider? dateTimeProvider = null) : INotificationHandler<NoticeIssuedEvent>
{
    public async Task Handle(NoticeIssuedEvent notification, CancellationToken cancellationToken)
    {
        var phoneRegistration = await dbContext.PhoneRegistrations
            .FirstOrDefaultAsync(x => x.RegistrationNumber == notification.RegistrationNumber, cancellationToken);

        if (phoneRegistration is null)
        {
            Console.WriteLine($"[Notifications] No phone registration found for {notification.RegistrationNumber}");
            return;
        }

        var message = $"Zawiadomienie {notification.NoticeId}: kara {notification.PenaltyAmount + notification.Surcharge} PLN za pojazd {notification.RegistrationNumber}";
        var result = await smsGateway.SendAsync(phoneRegistration.PhoneNumber, message, cancellationToken);

        var sentAt = dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var smsLog = SmsLog.Create(phoneRegistration.PhoneNumber, message, sentAt, result.Success, result.Error);
        dbContext.SmsLogs.Add(smsLog);
        await dbContext.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new SmsSentEvent(phoneRegistration.PhoneNumber, message, result.Success, sentAt), cancellationToken);
    }
}

