using System.Net.Http.Json;
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
            var unsyncedBatch = await dbContext.Notices
                .Where(n => n.SyncedAt == null)
                .OrderBy(n => n.IssuedAt)
                .Take(BatchSize)
                .ToListAsync(ct);

            if (unsyncedBatch.Count == 0)
            {
                break;
            }

            var now = DateTime.UtcNow;
            var payload = new ExternalNoticesBatchRequest(
                Guid.NewGuid(),
                now,
                unsyncedBatch.Select(n => new ExternalNoticeItemDto(
                    n.Id.Value,
                    n.InspectionId.Value,
                    n.RegistrationNumber.Value,
                    n.PenaltyAmount,
                    n.Surcharge,
                    n.TotalAmount,
                    n.Status.ToString(),
                    n.IssuedAt)).ToList());

            var response = await client.PostAsJsonAsync("/api/external/notices/batch", payload, ct);
            response.EnsureSuccessStatusCode();

            foreach (var notice in unsyncedBatch)
            {
                notice.MarkAsSynced(now);
            }

            await dbContext.SaveChangesAsync(ct);
            totalExported += unsyncedBatch.Count;

            logger.LogInformation("Wysłano paczkę {Count} zawiadomień do systemu zewnętrznego.", unsyncedBatch.Count);
        }

        return totalExported;
    }
}