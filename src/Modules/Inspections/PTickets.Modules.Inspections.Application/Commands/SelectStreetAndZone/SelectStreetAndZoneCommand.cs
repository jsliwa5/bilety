using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.SelectStreetAndZone;

public record SelectStreetAndZoneCommand(Guid SessionId, Guid ZoneId, Guid StreetId) : IRequest;

