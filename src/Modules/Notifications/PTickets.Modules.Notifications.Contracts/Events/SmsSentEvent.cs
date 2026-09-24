namespace PTickets.Modules.Notifications.Contracts.Events;

using MediatR;

public record SmsSentEvent(
    string PhoneNumber,
    string Message,
    bool Success,
    DateTime SentAt) : INotification;
