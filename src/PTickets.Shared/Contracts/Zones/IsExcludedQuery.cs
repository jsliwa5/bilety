namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record IsExcludedQuery(ZoneId ZoneId, StreetId StreetId, DateTime DateTime) : IRequest<bool>;
