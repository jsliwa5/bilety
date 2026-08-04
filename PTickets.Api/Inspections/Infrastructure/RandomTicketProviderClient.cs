using PTickets.Api.Common;
using PTickets.Api.Inspections.Contracts;
using PTickets.Api.Inspections.Data;

namespace PTickets.Api.Inspections.Infrastructure;

public class RandomTicketProviderClient : ITicketProviderClient
{
    public Task<TicketCheckResult> CheckTicketAsync(
        RegistrationNumber registrationNumber,
        StreetId streetId,
        CancellationToken ct = default)
    {
        // Losujemy 0 lub 1 (50% szans)
        var isValid = Random.Shared.Next(0, 2) == 1;
        var now = DateTime.UtcNow;

        TicketCheckResult result = isValid
            ? new TicketCheckResult(
                true,
                now.AddHours(-1),
                now.AddHours(2),
                "Bilet opłacony w parkomacie / aplikacji mobilnej.")
            : new TicketCheckResult(
                false,
                null,
                null,
                "Brak aktywnego biletu dla podanego numeru rejestracyjnego.");

        return Task.FromResult(result);
    }

   
}
