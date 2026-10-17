namespace ExternalNoticesSystem.Api.Common.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid NoticeId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage CreateWebhookEvent(Guid noticeId, string action, string? reason = null)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            NoticeId = noticeId,
            Action = action,
            Reason = reason,
            OccurredAt = DateTime.UtcNow,
            RetryCount = 0
        };
    }

    public void MarkAsProcessed(DateTime processedAt)
    {
        ProcessedAt = processedAt;
        LastError = null;
    }

    public void MarkAsFailed(string error)
    {
        RetryCount++;
        LastError = error;
    }
}

