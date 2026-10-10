namespace PTickets.Modules.Tickets.Infrastructure.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Tickets.Application.Commands;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public static class ResidentCardEndpoints
{
    public static IEndpointRouteBuilder MapResidentCardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/resident-cards");

        group.MapPost("/", async (IssueResidentCardRequest request, IMediator mediator) =>
        {
            var command = new IssueResidentCardCommand(
                RegistrationNumber.Create(request.RegistrationNumber),
                new StreetId(request.StreetId),
                DateTime.SpecifyKind(request.ValidFrom, DateTimeKind.Utc),
                DateTime.SpecifyKind(request.ValidTo, DateTimeKind.Utc));

            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        });

        return endpoints;
    }
}

public record IssueResidentCardRequest(string RegistrationNumber, Guid StreetId, DateTime ValidFrom, DateTime ValidTo);

