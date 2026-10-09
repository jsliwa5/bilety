using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.Common.Exceptions;
using PTickets.Shared;

namespace PTickets.Modules.Notices.CancelNotice;
public static class CancelNoticeEndpoint
{
    public static void MapCancelNoticeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/notices/{id:guid}/cancel", async (Guid id, NoticesDbContext dbContext, CancellationToken ct) =>
        {
            var notice = await dbContext.Notices.FirstOrDefaultAsync(n => n.Id == new NoticeId(id), ct);
            if (notice is null)
            {
                throw new NoticeNotFoundException();
            }

            notice.Cancel();

            await dbContext.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}

