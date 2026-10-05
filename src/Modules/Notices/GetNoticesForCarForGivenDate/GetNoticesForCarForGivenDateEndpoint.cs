using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notices.Data;
using PTickets.Modules.Notices.Contracts;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Notices.GetNoticesForCarForGivenDate;

public static class GetNoticesForCarForGivenDateEndpoint
{
    public static void MapGetNoticesForCarForGivenDateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notices/car/{registrationNumber}/date/{date:datetime}", async (string registrationNumber, DateTime date, NoticesDbContext dbContext, CancellationToken ct) =>
        {
            var regNum = new RegistrationNumber(registrationNumber);
            var day = date.Date;
            var nextDay = day.AddDays(1);

            var notices = await dbContext.Notices
                .Where(n => n.RegistrationNumber == regNum && n.IssuedAt >= day && n.IssuedAt < nextDay)
                .ToListAsync(ct);

            if (!notices.Any())
            {
                return Results.NotFound();
            }

            var noticeDtos = notices.Select(n => new NoticeDto(
                n.Id.Value,
                n.InspectionId.Value,
                n.RegistrationNumber.Value,
                n.TotalAmount,
                n.IssuedAt
            )).ToList();

            return Results.Ok(noticeDtos);
        });
    }
}

