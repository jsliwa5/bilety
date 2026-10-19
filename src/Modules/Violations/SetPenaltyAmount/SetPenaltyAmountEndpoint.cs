namespace PTickets.Modules.Violations.SetPenaltyAmount;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations.Common.Exceptions;
using PTickets.Shared;
using PTickets.Shared.Abstractions;

public static class SetPenaltyAmountEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/violation-types/{id:guid}/penalty-amount",
            (Guid id, SetPenaltyAmountRequest request, ViolationsDbContext dbContext, IDateTimeProvider? dateTimeProvider, CancellationToken ct)
                => HandleAsync(id, request, dbContext, dateTimeProvider, ct));
    }

    internal static async Task<IResult> HandleAsync(
        Guid id,
        SetPenaltyAmountRequest request,
        ViolationsDbContext dbContext,
        IDateTimeProvider? dateTimeProvider,
        CancellationToken ct)
    {
        var violationTypeId = new ViolationTypeId(id);
        var exists = await dbContext.ViolationTypes.AnyAsync(v => v.Id == violationTypeId, ct);
        if (!exists)
        {
            throw new ViolationTypeNotFoundException(id);
        }

        var now = dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var effectiveFrom = DateTime.SpecifyKind(request.EffectiveFrom, DateTimeKind.Utc);
        var penaltyAmount = PenaltyAmount.Create(violationTypeId, request.Amount, effectiveFrom, now);
        dbContext.PenaltyAmounts.Add(penaltyAmount);
        await dbContext.SaveChangesAsync(ct);

        return Results.Created($"/api/violation-types/{id}/penalty-amount/{penaltyAmount.Id}", new { id = penaltyAmount.Id });
    }
}

