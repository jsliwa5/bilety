namespace PTickets.Shared.Contracts.Zones;

using MediatR;

public record IsPaidAtDateTimeQuery(StreetId StreetId, DateTime DateTime) : IRequest<bool>;
