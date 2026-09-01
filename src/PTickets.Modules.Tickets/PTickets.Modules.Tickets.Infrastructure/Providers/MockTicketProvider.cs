namespace PTickets.Modules.Tickets.Infrastructure.Providers;

using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class MockTicketProvider : ITicketProvider
{
    public string ProviderName => "MockProvider";

    public Task<TicketCheckResult> CheckAsync(
        RegistrationNumber registration,
        StreetId streetId,
        DateTime at,
        CancellationToken ct = default)
    {
        var isValid = Random.Shared.Next(2) == 0;
        if (isValid)
        {
            var validFrom = at.AddHours(-1);
            var validTo = at.AddHours(1);
            return Task.FromResult(TicketCheckResult.Valid(validFrom, validTo, ProviderName));
        }

        return Task.FromResult(TicketCheckResult.Invalid("Brak biletu w MockProvider"));
    }
}

