namespace PTickets.Shared.Contracts.InspectorTracking;

using MediatR;

public record GetAssignedZoneQuery(InspectorId InspectorId) : IRequest<ZoneId?>;
