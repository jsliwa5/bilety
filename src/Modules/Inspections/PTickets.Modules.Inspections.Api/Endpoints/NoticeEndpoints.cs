using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Inspections.Application.Queries.GetNotice;

namespace PTickets.Modules.Inspections.Api.Endpoints;

public static class NoticeEndpoints
{
    public static void MapNoticeEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/notices/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var notice = await mediator.Send(new GetNoticeQuery(id));
            return notice != null ? Results.Ok(notice) : Results.NotFound();
        });
    }
}

