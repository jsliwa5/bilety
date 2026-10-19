namespace PTickets.Modules.Violations.CreateViolationType;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Modules.Violations.Common.Exceptions;

public static class CreateViolationTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/violation-types", HandleAsync);
    }

    internal static async Task<IResult> HandleAsync(
        CreateViolationTypeRequest request,
        ViolationsDbContext dbContext,
        CancellationToken ct)
    {
        var violationType = ViolationType.Create(request.Name, request.Description);
        var normalizedName = violationType.Name.ToLowerInvariant();

        var duplicateExists = await dbContext.ViolationTypes
            .AnyAsync(v => v.Name.ToLower() == normalizedName, ct);

        if (duplicateExists)
        {
            throw new DuplicateViolationTypeNameException(violationType.Name);
        }

        dbContext.ViolationTypes.Add(violationType);
        await dbContext.SaveChangesAsync(ct);

        return Results.Created($"/api/violation-types/{violationType.Id.Value}", new { id = violationType.Id.Value });
    }
}

