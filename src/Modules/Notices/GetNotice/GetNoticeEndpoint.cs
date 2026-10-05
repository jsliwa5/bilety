using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PTickets.Shared;
using PTickets.Modules.Notices.Data;
using PTickets.Modules.Notices.Contracts;

namespace PTickets.Modules.Notices.GetNotice;

public static class GetNoticeEndpoint
{
    public static void MapGetNoticeEndpoint(this IEndpointRouteBuilder app) 
    {
        app.MapGet("/notices/{noticeId:guid}", async (Guid noticeId, NoticesDbContext context, CancellationToken ct) =>
        {
            var notice = await context.Notices.FirstOrDefaultAsync(n => n.Id == new NoticeId(noticeId), ct);

            if (notice is null)
            {
                return Results.NotFound();
            }

            var noticeDto = new NoticeDto(
                    notice.Id.Value,
                    notice.InspectionId.Value,
                    notice.RegistrationNumber.Value,
                    notice.TotalAmount,
                    notice.IssuedAt
                );

            return Results.Ok(noticeDto);
        });
    }
}
