using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PTickets.Modules.Notices.Common.Data;
using PTickets.Modules.Notices.Common.Exceptions;
using PTickets.Shared;
using Microsoft.EntityFrameworkCore;

namespace PTickets.Modules.Notices.PayNotice;

public static class PayNoticeEndpoint
{
    public static void MapPayNoticeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/notices/{id:guid}/pay", async (Guid id, NoticesDbContext dbContext, CancellationToken ct) =>
        {
            var notice = await dbContext.Notices.FirstOrDefaultAsync(n => n.Id == new NoticeId(id), ct);
            if (notice is null)
            {
                throw new NoticeNotFoundException();
            }

            notice.MarkAsPaid();

            await dbContext.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}

