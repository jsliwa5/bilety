
using PTickets.Api.Common;
using PTickets.Api.Inspections.Data;

namespace PTickets.Api.Inspections.Contracts;

public interface ITicketProviderClient
{
    public Task<TicketCheckResult> CheckTicketAsync(
        RegistrationNumber registrationNumber,
        ZoneId zoneId,
        CancellationToken ct = default);
}