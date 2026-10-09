namespace PTickets.Modules.Violations.CreateSurchargeTier;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Common.Data;

public static class CreateSurchargeTierEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/surcharge-tiers", async (CreateSurchargeTierRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var tier = SurchargeTier.Create(request.MinMinutes, request.MaxMinutes, request.Amount);
            dbContext.SurchargeTiers.Add(tier);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/surcharge-tiers/{tier.Id.Value}", new { id = tier.Id.Value });
        });
    }
}
