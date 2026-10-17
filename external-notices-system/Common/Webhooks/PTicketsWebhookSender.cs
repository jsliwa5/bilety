namespace ExternalNoticesSystem.Api.Common.Webhooks;

using System.Net.Http.Json;

public record NoticeWebhookPayload(
    Guid EventId,
    Guid NoticeId,
    string Action,
    string? Reason,
    DateTime OccurredAt);

public class PTicketsWebhookSender(IHttpClientFactory httpClientFactory)
{
    public async Task SendStatusChangedAsync(
        Guid eventId,
        Guid noticeId,
        string action,
        string? reason,
        DateTime occurredAt,
        CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("PTicketsWebhookClient");

        var payload = new NoticeWebhookPayload(
            eventId,
            noticeId,
            action,
            reason,
            occurredAt);

        var response = await client.PostAsJsonAsync("/api/notices/webhook", payload, ct);
        response.EnsureSuccessStatusCode();
    }
}

