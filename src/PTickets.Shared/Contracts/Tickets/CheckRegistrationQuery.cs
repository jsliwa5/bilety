namespace PTickets.Shared.Contracts.Tickets;

using MediatR;
using PTickets.Shared.ValueObjects;

public record CheckRegistrationQuery(
    RegistrationNumber RegistrationNumber,
    StreetId StreetId,
    DateTime At) : IRequest<TicketCheckResult>;
