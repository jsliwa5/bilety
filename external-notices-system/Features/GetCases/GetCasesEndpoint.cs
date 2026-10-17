namespace ExternalNoticesSystem.Api.Features.GetCases;

using ExternalNoticesSystem.Api.Common.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public static class GetCasesEndpoint
{
    public static void MapGetCasesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cases", HandleAsync)
           .WithTags("2. Zarządzanie sprawami i Outbox");
    }

    private static async Task<IResult> HandleAsync(
        string? status,
        string? registrationNumber,
        ExternalNoticesDbContext db,
        CancellationToken ct)
    {
        var query = db.NoticeCases.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(registrationNumber))
            query = query.Where(c => c.RegistrationNumber.Contains(registrationNumber.ToUpperInvariant()));

        var cases = await query.OrderByDescending(c => c.IssuedAt).ToListAsync(ct);
        var now = DateTime.UtcNow;

        var result = cases.Select(c => new
        {
            c.NoticeId,
            c.InspectionId,
            c.RegistrationNumber,
            c.Status,
            c.BaseAmount,
            c.DiscountedAmount,
            IsDiscountActive = c.IsDiscountActive(now),
            CurrentAmountToPay = c.GetCurrentAmountToPay(now),
            c.DiscountDeadline,
            c.PaymentDueDate,
            c.PaidAmount,
            c.AppealReason,
            c.IssuedAt,
            c.ReceivedAt,
            c.UpdatedAt
        });

        return Results.Ok(result);
    }
}

