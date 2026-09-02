namespace PTickets.Modules.Tickets.Infrastructure.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class TicketsEndpoints
{
    public static IEndpointRouteBuilder MapTicketsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tickets");

        group.MapPost("/import", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

        return endpoints;
    }
}

