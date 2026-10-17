namespace ExternalNoticesSystem.Api.Features.SendToDebtCollection;

using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class SendToDebtCollectionEndpoint
{
    public static void MapSendToDebtCollectionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cases/{noticeId:guid}/send-to-debt-collection", HandleAsync)
           .WithTags("3. Akcje i Webhooki (Zewnętrzny System -> PTickets)");
    }

    private static async Task<IResult> HandleAsync(
        Guid noticeId,
        ExternalNoticesDbContext db,
        CancellationToken ct)
    {
        var noticeCase = await db.NoticeCases.FirstOrDefaultAsync(c => c.NoticeId == noticeId, ct);
        if (noticeCase is null)
            return Results.NotFound("Nie znaleziono sprawy.");

        try
        {
            noticeCase.SendToDebtCollection(DateTime.UtcNow);

            db.OutboxMessages.Add(OutboxMessage.CreateWebhookEvent(noticeId, "SENT_TO_DEBT_COLLECTION", "Przekazano do windykacji"));

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                noticeId,
                newStatus = noticeCase.Status,
                outboxQueued = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}

