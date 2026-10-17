using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PTickets.Modules.Notices.ExternalSync;

public record ExternalNoticeItemDto(
    Guid NoticeId,
    Guid InspectionId,
    string RegistrationNumber,
    decimal PenaltyAmount,
    decimal Surcharge,
    decimal TotalAmount,
    string Status,
    DateTime IssuedAt);

public record ExternalNoticesBatchRequest(
    Guid BatchId,
    DateTime ExportedAt,
    List<ExternalNoticeItemDto> Notices);

public class DailyNoticesExportService(
    NoticesDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DailyNoticesExportService> logger)
{
    private const int BatchSize = 100;

    public async Task<int> ExportUnsyncedNoticesAsync(CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("ExternalNoticesClient");
        var apiKey = configuration["ExternalNoticesSystem:ApiKey"] ?? "secret-webhook-key-123";

        if (!client.DefaultRequestHeaders.Contains("X-Api-Key"))
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }

        var totalExported = 0;

        while (!ct.IsCancellationRequested)
        {
            var pendingMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccurredAt)
                .Take(BatchSize)
                .ToListAsync(ct);

            if (pendingMessages.Count == 0)
            {
                break;
            }

            var noticesDto = pendingMessages
                .Select(m => JsonSerializer.Deserialize<ExternalNoticeItemDto>(m.PayloadJson)!)
                .ToList();

            var now = DateTime.UtcNow;
            var batchId = Guid.NewGuid();
            var payload = new ExternalNoticesBatchRequest(batchId, now, noticesDto);

            try
            {
                logger.LogInformation("[OUTBOX] Wysyłanie paczki {BatchId} ({Count} wiadomości) do systemu zewnętrznego...", batchId, pendingMessages.Count);

                // Żądanie chronione przez Polly (Retry + CircuitBreaker + Timeout)
                var response = await client.PostAsJsonAsync("/api/external/notices/batch", payload, ct);
                response.EnsureSuccessStatusCode();

                foreach (var msg in pendingMessages)
                {
                    msg.MarkAsProcessed(now);
                }

                await dbContext.SaveChangesAsync(ct);
                totalExported += pendingMessages.Count;

                logger.LogInformation("[OUTBOX] Paczka {BatchId} wysłana i oznaczona jako Processed.", batchId);
            }
            catch (Exception ex)
            {
                foreach (var msg in pendingMessages)
                {
                    msg.MarkAsFailed(ex.Message);
                }

                await dbContext.SaveChangesAsync(ct);

                logger.LogError(ex, "[OUTBOX] Błąd wysyłki paczki {BatchId} po wyczerpaniu prób Polly.", batchId);
                throw;
            }
        }

        return totalExported;
    }
}