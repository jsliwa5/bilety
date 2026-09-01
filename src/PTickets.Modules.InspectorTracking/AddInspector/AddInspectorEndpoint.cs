using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.InspectorTracking.Data;
using PTickets.Shared;
using PTickets.Shared.Contracts.Zones;

namespace PTickets.Modules.InspectorTracking.AddInspector;

public static class AddInspectorEndpoint
{
    public static void MapAddInspector(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inspectors", AddInspector)
            .WithName("AddInspector")
            .Produces<AddInspectorResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> AddInspector(
        AddInspectorRequest request,
        InspectorTrackingDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Results.BadRequest("Imię inspektora nie może być puste.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Results.BadRequest("Nazwisko inspektora nie może być puste.");

        var inspector = Inspector.Create(request.FirstName, request.LastName);

        await dbContext.Inspectors.AddAsync(inspector, ct);
        await dbContext.SaveChangesAsync(ct);

        var response = new AddInspectorResponse(inspector.Id.Value);
        return Results.Created($"/api/inspectors/{inspector.Id.Value}", response);
    }
}
