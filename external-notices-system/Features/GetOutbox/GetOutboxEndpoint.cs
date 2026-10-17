namespace ExternalNoticesSystem.Api.Features.GetOutbox;

using ExternalNoticesSystem.Api.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class GetOutboxEndpoint
{
    public static void MapGetOutboxEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/outbox", HandleAsync)
           .WithTags("2. Zarządzanie sprawami i Outbox");
    }

    private static async Task<IResult> HandleAsync(
        ExternalNoticesDbContext db,
        CancellationToken ct)
    {
        var messages = await db.OutboxMessages
            .AsNoTracking()
            .OrderByDescending(m => m.OccurredAt)
            .ToListAsync(ct);

        return Results.Ok(messages);
    }
}

