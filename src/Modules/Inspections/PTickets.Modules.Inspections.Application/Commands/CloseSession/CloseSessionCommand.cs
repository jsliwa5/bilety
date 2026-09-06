using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.CloseSession;

public record CloseSessionCommand(Guid SessionId) : IRequest;

