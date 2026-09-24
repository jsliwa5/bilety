using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Inspections.Application.Commands.StartSession;
using PTickets.Modules.Inspections.Application.Commands.SelectStreetAndZone;
using PTickets.Modules.Inspections.Application.Commands.CloseSession;
using PTickets.Modules.Inspections.Application.Queries.GetSession;

namespace PTickets.Modules.Inspections.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/sessions");

        group.MapPost("/", async ([FromBody] StartSessionCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(id);
        });

        //group.MapPut("/{sessionId:guid}/street", async (Guid sessionId, [FromBody] SelectStreetAndZoneCommand command, IMediator mediator) =>
        //{
        //    await mediator.Send(command with { SessionId = sessionId });
        //    return Results.Ok();
        //});

        group.MapPost("/{sessionId:guid}/close", async (Guid sessionId, IMediator mediator) =>
        {
            await mediator.Send(new CloseSessionCommand(sessionId));
            return Results.Ok();
        });

        group.MapGet("/{sessionId:guid}", async (Guid sessionId, IMediator mediator) =>
        {
            var session = await mediator.Send(new GetSessionQuery(sessionId));
            return session != null ? Results.Ok(session) : Results.NotFound();
        });
    }
}

