namespace PTickets.Shared.Contracts.Notifications;

using MediatR;

public record SmsSentEvent(
    string PhoneNumber,
    string Message,
    bool Success,
    DateTime SentAt) : INotification;
