using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PTickets.Modules.Notices.Common.Exceptions;
using PTickets.Shared;

namespace PTickets.Modules.Notices.ExternalSync;

public record NoticeStatusWebhookPayload(
    Guid EventId,
    Guid NoticeId,
    string Action,
    string? Reason,
    DateTime OccurredAt);

public static class ExternalSyncEndpoints
{
    public static void MapExternalSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/notices/export-daily", async (DailyNoticesExportService exportService, CancellationToken ct) =>
        {
            var exportedCount = await exportService.ExportUnsyncedNoticesAsync(ct);
            return Results.Ok(new
            {
                message = "Eksport do systemu zewnętrznego zakończony pomyślnie.",
                exportedCount
            });
        });

        app.MapPost("/api/notices/webhook", async (
            HttpContext httpContext,
            [FromBody] NoticeStatusWebhookPayload payload,
            NoticesDbContext dbContext,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("NoticesWebhook");
            var expectedKey = configuration["ExternalNoticesSystem:ApiKey"] ?? "secret-webhook-key-123";

            if (!httpContext.Request.Headers.TryGetValue("X-Api-Key", out var providedKey) || providedKey != expectedKey)
            {
                logger.LogWarning("Odrzucono webhook z nieprawidłowym kluczem X-Api-Key.");
                return Results.Unauthorized();
            }

            var notice = await dbContext.Notices
                .FirstOrDefaultAsync(n => n.Id == new NoticeId(payload.NoticeId), ct);

            if (notice is null)
            {
                throw new NoticeNotFoundException();
            }

            switch (payload.Action.ToUpperInvariant())
            {
                case "PAID":
                    notice.MarkAsPaid();
                    break;
                case "APPEALED":
                    notice.MarkAsAppealed();
                    break;
                case "APPEAL_APPROVED":
                case "CANCELLED":
                    notice.Cancel();
                    break;
                case "APPEAL_REJECTED":
                    notice.RejectAppeal();
                    break;
                case "SENT_TO_DEBT_COLLECTION":
                    notice.SendToDebtCollection();
                    break;
                default:
                    return Results.BadRequest(new { error = $"Nieznana akcja webhooka: {payload.Action}" });
            }

            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation(
                "Odebrano webhook ({Action}) dla wezwania {NoticeId}. Nowy status w PTickets: {Status}",
                payload.Action, notice.Id.Value, notice.Status);

            return Results.Ok(new
            {
                noticeId = notice.Id.Value,
                newStatus = notice.Status.ToString()
            });
        });
    }
}