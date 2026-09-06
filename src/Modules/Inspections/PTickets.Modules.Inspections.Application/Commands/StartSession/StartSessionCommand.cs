using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.StartSession;

public record StartSessionCommand(Guid InspectorId) : IRequest<Guid>;

