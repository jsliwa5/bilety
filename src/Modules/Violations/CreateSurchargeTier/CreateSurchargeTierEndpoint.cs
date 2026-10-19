namespace PTickets.Modules.Violations.CreateSurchargeTier;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations.Common.Exceptions;

public static class CreateSurchargeTierEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/surcharge-tiers", HandleAsync);
    }

    internal static async Task<IResult> HandleAsync(
        CreateSurchargeTierRequest request,
        ViolationsDbContext dbContext,
        CancellationToken ct)
    {
        var newTier = SurchargeTier.Create(request.MinMinutes, request.MaxMinutes, request.Amount);
        var existingTiers = await dbContext.SurchargeTiers.AsNoTracking().ToListAsync(ct);

        if (existingTiers.Any(t => t.OverlapsWith(newTier)))
        {
            throw new OverlappingSurchargeTierException();
        }

        dbContext.SurchargeTiers.Add(newTier);
        await dbContext.SaveChangesAsync(ct);

        return Results.Created($"/api/surcharge-tiers/{newTier.Id.Value}", new { id = newTier.Id.Value });
    }
}

