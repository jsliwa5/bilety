namespace PTickets.Modules.Violations.CreateViolationType;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Common.Data;

public static class CreateViolationTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/violation-types", async (CreateViolationTypeRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var violationType = ViolationType.Create(request.Name, request.Description);
            dbContext.ViolationTypes.Add(violationType);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/violation-types/{violationType.Id.Value}", new { id = violationType.Id.Value });
        });
    }
}
