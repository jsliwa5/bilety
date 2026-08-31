namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record ZoneExistsQuery(ZoneId ZoneId) : IRequest<bool>;
