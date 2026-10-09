using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.InspectorTracking.Common.Data;
using PTickets.Shared;

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
        var inspector = Inspector.Create(request.FirstName, request.LastName);

        await dbContext.Inspectors.AddAsync(inspector, ct);
        await dbContext.SaveChangesAsync(ct);

        var response = new AddInspectorResponse(inspector.Id.Value);
        return Results.Created($"/api/inspectors/{inspector.Id.Value}", response);
    }
}


