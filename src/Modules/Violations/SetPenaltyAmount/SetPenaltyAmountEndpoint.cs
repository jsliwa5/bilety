namespace PTickets.Modules.Violations.SetPenaltyAmount;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Shared;

public static class SetPenaltyAmountEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/violation-types/{id:guid}/penalty-amount", async (Guid id, SetPenaltyAmountRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var violationTypeId = new ViolationTypeId(id);
            var exists = await dbContext.ViolationTypes.AnyAsync(v => v.Id == violationTypeId, ct);
            if (!exists)
            {
                return Results.NotFound(new { message = $"Violation type with ID {id} was not found." });
            }

            var penaltyAmount = PenaltyAmount.Create(violationTypeId, request.Amount, request.EffectiveFrom);
            dbContext.PenaltyAmounts.Add(penaltyAmount);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/violation-types/{id}/penalty-amount/{penaltyAmount.Id}", new { id = penaltyAmount.Id });
        });
    }
}
