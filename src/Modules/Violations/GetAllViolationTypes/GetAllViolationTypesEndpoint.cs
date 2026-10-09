namespace PTickets.Modules.Violations.GetAllViolationTypes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Contracts;

public static class GetAllViolationTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/violation-types", async (IViolationsModule module, CancellationToken ct) =>
        {
            var violationTypes = await module.GetAllViolationTypesAsync(ct);
            return Results.Ok(violationTypes);
        });
    }
}
