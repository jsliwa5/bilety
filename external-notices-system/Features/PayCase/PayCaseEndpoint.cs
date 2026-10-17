namespace ExternalNoticesSystem.Api.Features.PayCase;

using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class PayCaseEndpoint
{
    public static void MapPayCaseEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cases/{noticeId:guid}/pay", HandleAsync)
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
            var now = DateTime.UtcNow;
            var discountApplied = noticeCase.IsDiscountActive(now);
            var paidAmount = noticeCase.Pay(now);

            db.OutboxMessages.Add(OutboxMessage.CreateWebhookEvent(noticeId, "PAID", $"Opłacono {paidAmount} PLN"));

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                noticeId,
                newStatus = noticeCase.Status,
                paidAmount,
                discountApplied,
                outboxQueued = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}

