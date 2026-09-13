namespace PTickets.Modules.Notifications.Domain;

public class SmsLog
{
    public Guid Id { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }

    private SmsLog() { }

    public static SmsLog Create(string phoneNumber, string message, DateTime sentAt, bool success, string? errorMessage = null)
    {
        return new SmsLog
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            Message = message,
            SentAt = sentAt,
            Success = success,
            ErrorMessage = errorMessage
        };
    }
}

