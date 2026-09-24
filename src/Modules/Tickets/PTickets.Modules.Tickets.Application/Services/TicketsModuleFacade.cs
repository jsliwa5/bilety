namespace PTickets.Modules.Tickets.Application.Services;

using PTickets.Modules.Tickets.Contracts;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class TicketsModuleFacade : ITicketsModule
{
    private readonly TicketVerificationService _verificationService;

    public TicketsModuleFacade(TicketVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    public Task<TicketCheckResult> CheckRegistrationAsync(
        RegistrationNumber registration,
        StreetId streetId,
        DateTime at,
        CancellationToken ct)
        => _verificationService.VerifyTicketAsync(registration, streetId, at, ct);
}
