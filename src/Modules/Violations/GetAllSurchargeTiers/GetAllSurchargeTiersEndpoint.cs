namespace PTickets.Modules.Violations.GetAllSurchargeTiers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations;

public static class GetAllSurchargeTiersEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/surcharge-tiers", async (ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var tiers = await dbContext.SurchargeTiers
                .AsNoTracking()
                .OrderBy(t => t.MinMinutes)
                .Select(t => new
                {
                    id = t.Id.Value,
                    minMinutes = t.MinMinutes,
                    maxMinutes = t.MaxMinutes,
                    amount = t.Amount
                })
                .ToListAsync(ct);

            return Results.Ok(tiers);
        });
    }
}
