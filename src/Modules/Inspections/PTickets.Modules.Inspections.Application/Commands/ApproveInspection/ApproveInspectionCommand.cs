using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.ApproveInspection;

public record ApproveInspectionCommand(Guid InspectionId) : IRequest;

