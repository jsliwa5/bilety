using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Notices.Application.Queries.GetNotice;
using PTickets.Modules.Notices.Domain;
using PTickets.Shared;

namespace PTickets.Modules.Notices.Endpoints;

public static class NoticeEndpoints
{
    public static void MapNoticeEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/notices/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var notice = await mediator.Send(new GetNoticeQuery(id));
            return notice != null ? Results.Ok(notice) : Results.NotFound();
        });

        builder.MapPost("/api/notices/{id:guid}/pay", async (Guid id, INoticeRepository repository, CancellationToken ct) =>
        {
            var notice = await repository.GetByIdAsync(new NoticeId(id), ct);
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

            await repository.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        builder.MapPost("/api/notices/{id:guid}/cancel", async (Guid id, INoticeRepository repository, CancellationToken ct) =>
        {
            var notice = await repository.GetByIdAsync(new NoticeId(id), ct);
            if (notice is null)
            {
                return Results.NotFound();
            }

            try
            {
                notice.Cancel();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await repository.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
