namespace PTickets.Shared.Contracts.InspectorTracking;

using MediatR;

public record InspectorExistsQuery(InspectorId InspectorId) : IRequest<bool>;
