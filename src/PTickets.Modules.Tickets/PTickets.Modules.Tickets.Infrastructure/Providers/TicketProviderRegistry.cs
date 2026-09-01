namespace PTickets.Modules.Tickets.Infrastructure.Providers;

using PTickets.Modules.Tickets.Domain;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class TicketProviderRegistry(IEnumerable<ITicketProvider> providers)
{
    public async Task<TicketCheckResult> QueryProvidersAsync(
        RegistrationNumber registration,
        StreetId streetId,
        DateTime at,
        CancellationToken ct = default)
    {
        foreach (var provider in providers)
        {
            var result = await provider.CheckAsync(registration, streetId, at, ct);
            if (result.IsValid)
            {
                return result;
            }
        }

        return TicketCheckResult.Invalid("Brak ważnego biletu");
    }
}

