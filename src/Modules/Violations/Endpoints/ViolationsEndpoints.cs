namespace PTickets.Modules.Violations.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Application.Dtos;
using PTickets.Modules.Violations.Domain;
using PTickets.Modules.Violations.Infrastructure.Persistence;
using PTickets.Shared;
using PTickets.Shared.Contracts.Violations;

public static class ViolationsEndpoints
{
    public static IEndpointRouteBuilder MapViolationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");

        group.MapPost("/violation-types", async (CreateViolationTypeRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var violationType = ViolationType.Create(request.Name, request.Description);
            dbContext.ViolationTypes.Add(violationType);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/violation-types/{violationType.Id.Value}", new { id = violationType.Id.Value });
        });

        group.MapGet("/violation-types", async (IMediator mediator, CancellationToken ct) =>
        {
            var violationTypes = await mediator.Send(new GetAllViolationTypesQuery(), ct);
            return Results.Ok(violationTypes);
        });

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

        group.MapPost("/surcharge-tiers", async (CreateSurchargeTierRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var tier = SurchargeTier.Create(request.MinMinutes, request.MaxMinutes, request.Amount);
            dbContext.SurchargeTiers.Add(tier);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/surcharge-tiers/{tier.Id.Value}", new { id = tier.Id.Value });
        });

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

        return endpoints;
    }
}
