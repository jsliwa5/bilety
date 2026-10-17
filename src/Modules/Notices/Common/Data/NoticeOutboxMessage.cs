namespace PTickets.Modules.Notices.Common.Data;

public class NoticeOutboxMessage
{
    public Guid Id { get; private set; }
    public Guid NoticeId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    private NoticeOutboxMessage() { }

    public static NoticeOutboxMessage Create(Guid noticeId, string type, string payloadJson, DateTime occurredAt)
    {
        return new NoticeOutboxMessage
        {
            Id = Guid.NewGuid(),
            NoticeId = noticeId,
            Type = type,
            PayloadJson = payloadJson,
            OccurredAt = occurredAt,
            ProcessedAt = null,
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

