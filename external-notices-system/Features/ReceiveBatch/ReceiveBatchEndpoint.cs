namespace ExternalNoticesSystem.Api.Features.ReceiveBatch;

using ExternalNoticesSystem.Api.Common;
using ExternalNoticesSystem.Api.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

public static class ReceiveBatchEndpoint
{
    public static void MapReceiveBatchEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/external/notices/batch", HandleAsync)
           .WithTags("1. Integracja Batch (PTickets -> Zewnętrzny System)");
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromBody] ExternalNoticesBatchRequest batch,
        ExternalNoticesDbContext db,
        ChaosState chaos,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("ReceiveBatch");

        var expectedKey = configuration["ApiKey"] ?? "secret-webhook-key-123";
        if (!httpContext.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != expectedKey)
        {
            return Results.Unauthorized();
        }

        if (chaos.TryConsumeFailure(out var remaining))
        {
            logger.LogWarning("[CHAOS] Symulowanie błędu 503 Service Unavailable. Pozostało: {Remaining}", remaining);
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        int added = 0;
        int skippedDuplicates = 0;

        foreach (var item in batch.Notices)
        {
            var exists = await db.NoticeCases.AnyAsync(c => c.NoticeId == item.NoticeId, ct);
            if (exists)
            {
                skippedDuplicates++;
                continue;
            }

            var noticeCase = ExternalNoticeCase.Create(
                item.NoticeId,
                item.InspectionId,
                item.RegistrationNumber,
                item.TotalAmount,
                item.Status,
                item.IssuedAt);

            db.NoticeCases.Add(noticeCase);
            added++;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Odebrano paczkę {BatchId}: dodano {Added}, duplikatów: {Duplicates}.",
            batch.BatchId, added, skippedDuplicates);

        return Results.Ok(new
        {
            batchId = batch.BatchId,
            received = batch.Notices.Count,
            added,
            skippedDuplicates
        });
    }
}

