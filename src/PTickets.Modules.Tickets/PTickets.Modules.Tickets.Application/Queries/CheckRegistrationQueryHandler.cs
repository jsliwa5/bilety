namespace PTickets.Modules.Tickets.Application.Queries;

using MediatR;
using PTickets.Modules.Tickets.Application.Services;
using PTickets.Shared.Contracts.Tickets;
using PTickets.Shared.ValueObjects;

public class CheckRegistrationQueryHandler(TicketVerificationService verificationService)
    : IRequestHandler<CheckRegistrationQuery, TicketCheckResult>
{
    public Task<TicketCheckResult> Handle(CheckRegistrationQuery request, CancellationToken cancellationToken)
    {
        return verificationService.VerifyTicketAsync(request.RegistrationNumber, request.StreetId, request.At, cancellationToken);
    }
}

