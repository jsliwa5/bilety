using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Inspections.Application.Commands.StartInspection;
using PTickets.Modules.Inspections.Application.Commands.CheckTicket;
using PTickets.Modules.Inspections.Application.Commands.AddViolation;
using PTickets.Modules.Inspections.Application.Commands.ConductSecondCheck;
using PTickets.Modules.Inspections.Application.Commands.AttachPhotos;
using PTickets.Modules.Inspections.Application.Commands.IssueNotice;
using PTickets.Modules.Inspections.Application.Commands.ApproveInspection;
using PTickets.Modules.Inspections.Application.Queries.GetInspection;

namespace PTickets.Modules.Inspections.Api.Endpoints;

public static class InspectionEndpoints
{
    public static void MapInspectionEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/inspections");

        group.MapPost("/", async ([FromBody] StartInspectionCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(id);
        });

        group.MapPost("/{id:guid}/check-ticket", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CheckTicketCommand(id));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/violations", async (Guid id, [FromBody] AddViolationCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { InspectionId = id });
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/second-check", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ConductSecondCheckCommand(id));
            return Results.Ok(result);
        });

        group.MapPost("/{id:guid}/photos", async (Guid id, [FromBody] AttachPhotosCommand command, IMediator mediator) =>
        {
            await mediator.Send(command with { InspectionId = id });
            return Results.Ok();
        });

        group.MapPost("/{id:guid}/notice", async (Guid id, IMediator mediator) =>
        {
            var noticeId = await mediator.Send(new IssueNoticeCommand(id));
            return Results.Ok(noticeId);
        });

        group.MapPost("/{id:guid}/approve", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new ApproveInspectionCommand(id));
            return Results.Ok();
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var inspection = await mediator.Send(new GetInspectionQuery(id));
            return inspection != null ? Results.Ok(inspection) : Results.NotFound();
        });
    }
}

