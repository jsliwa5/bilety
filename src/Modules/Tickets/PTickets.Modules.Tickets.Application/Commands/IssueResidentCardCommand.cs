namespace PTickets.Modules.Tickets.Application.Commands;

using MediatR;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public record IssueResidentCardCommand(
    RegistrationNumber RegistrationNumber,
    StreetId StreetId,
    DateTime ValidFrom,
    DateTime ValidTo) : IRequest<Guid>;

