using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.StartInspection;

public record StartInspectionCommand(Guid SessionId, string RegistrationNumber, double Latitude, double Longitude, Guid ZoneId, Guid StreetId) : IRequest<Guid>;

