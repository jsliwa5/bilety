using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.AddViolation;

public record AddViolationCommand(Guid InspectionId, Guid ViolationTypeId) : IRequest;

