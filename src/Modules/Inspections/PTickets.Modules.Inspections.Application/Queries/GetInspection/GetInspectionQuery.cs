using MediatR;

namespace PTickets.Modules.Inspections.Application.Queries.GetInspection;

public record GetInspectionQuery(Guid InspectionId) : IRequest<InspectionDto?>;

