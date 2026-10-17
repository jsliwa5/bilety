namespace ExternalNoticesSystem.Api.Features.ResolveAppeal;

using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public record ResolveAppealRequest(bool Approved, string? DecisionNote);

public static class ResolveAppealEndpoint
{
    public static void MapResolveAppealEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cases/{noticeId:guid}/resolve-appeal", HandleAsync)
           .WithTags("3. Akcje i Webhooki (Zewnętrzny System -> PTickets)");
    }

    private static async Task<IResult> HandleAsync(
        Guid noticeId,
        [FromBody] ResolveAppealRequest request,
        ExternalNoticesDbContext db,
        CancellationToken ct)
    {
        var noticeCase = await db.NoticeCases.FirstOrDefaultAsync(c => c.NoticeId == noticeId, ct);
        if (noticeCase is null)
            return Results.NotFound("Nie znaleziono sprawy.");

        try
        {
            var webhookAction = noticeCase.ResolveAppeal(request.Approved, DateTime.UtcNow);

            db.OutboxMessages.Add(OutboxMessage.CreateWebhookEvent(noticeId, webhookAction, request.DecisionNote));

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

