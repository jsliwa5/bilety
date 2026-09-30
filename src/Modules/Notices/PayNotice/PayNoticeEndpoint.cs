using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PTickets.Modules.Notices.Data;
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
                return Results.NotFound();
            }

            try
            {
                notice.MarkAsPaid();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await dbContext.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
