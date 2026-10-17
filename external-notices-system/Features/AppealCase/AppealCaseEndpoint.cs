namespace ExternalNoticesSystem.Api.Features.AppealCase;

using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public record AppealRequest(string Reason);

public static class AppealCaseEndpoint
{
    public static void MapAppealCaseEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cases/{noticeId:guid}/appeal", HandleAsync)
           .WithTags("3. Akcje i Webhooki (Zewnętrzny System -> PTickets)");
    }

    private static async Task<IResult> HandleAsync(
        Guid noticeId,
        [FromBody] AppealRequest request,
        ExternalNoticesDbContext db,
        CancellationToken ct)
    {
        var noticeCase = await db.NoticeCases.FirstOrDefaultAsync(c => c.NoticeId == noticeId, ct);
        if (noticeCase is null)
            return Results.NotFound("Nie znaleziono sprawy.");

        try
        {
            noticeCase.Appeal(request.Reason, DateTime.UtcNow);

            db.OutboxMessages.Add(OutboxMessage.CreateWebhookEvent(noticeId, "APPEALED", request.Reason));

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                noticeId,
                newStatus = noticeCase.Status,
                appealReason = noticeCase.AppealReason,
                outboxQueued = true
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}

