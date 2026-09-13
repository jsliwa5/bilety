namespace PTickets.Modules.Notifications.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Notifications.Domain;
using PTickets.Modules.Notifications.Infrastructure.Persistence;
using PTickets.Shared.Abstractions;
using PTickets.Shared.ValueObjects;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");

        group.MapPost("/phone-registrations", async (
            PhoneRegistrationRequest request,
            NotificationsDbContext dbContext,
            IDateTimeProvider? dateTimeProvider,
            CancellationToken ct) =>
        {
            var regNumber = new RegistrationNumber(request.RegistrationNumber);
            var now = dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
            var phoneRegistration = PhoneRegistration.Create(regNumber, request.PhoneNumber, now);

            dbContext.PhoneRegistrations.Add(phoneRegistration);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($"/api/phone-registrations/{phoneRegistration.Id}", new
            {
                id = phoneRegistration.Id,
                registrationNumber = phoneRegistration.RegistrationNumber.Value,
                phoneNumber = phoneRegistration.PhoneNumber,
                registeredAt = phoneRegistration.RegisteredAt
            });
        });

        group.MapGet("/sms-logs", async (
            string? registrationNumber,
            NotificationsDbContext dbContext,
            CancellationToken ct) =>
        {
            if (!string.IsNullOrWhiteSpace(registrationNumber))
            {
                RegistrationNumber regNumber;
                try
                {
                    regNumber = new RegistrationNumber(registrationNumber);
                }
                catch (ArgumentException)
                {
                    return Results.Ok(Array.Empty<SmsLog>());
                }

                var phoneNumbers = await dbContext.PhoneRegistrations
                    .AsNoTracking()
                    .Where(p => p.RegistrationNumber == regNumber)
                    .Select(p => p.PhoneNumber)
                    .Distinct()
                    .ToListAsync(ct);

                if (phoneNumbers.Count == 0)
                {
                    return Results.Ok(Array.Empty<SmsLog>());
                }

                var filteredLogs = await dbContext.SmsLogs
                    .AsNoTracking()
                    .Where(s => phoneNumbers.Contains(s.PhoneNumber))
                    .OrderByDescending(s => s.SentAt)
                    .ToListAsync(ct);

                return Results.Ok(filteredLogs);
            }

            var logs = await dbContext.SmsLogs
                .AsNoTracking()
                .OrderByDescending(s => s.SentAt)
                .ToListAsync(ct);

            return Results.Ok(logs);
        });

        return endpoints;
    }
}

public record PhoneRegistrationRequest(string RegistrationNumber, string PhoneNumber);

