namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record StreetBelongsToZoneQuery(ZoneId ZoneId, StreetId StreetId) : IRequest<bool>;
