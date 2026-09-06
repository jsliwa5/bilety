namespace PTickets.Modules.Inspections.Application.Queries.GetSession;

public record SessionDto(Guid Id, Guid InspectorId, Guid? ZoneId, Guid? StreetId, DateTime StartedAt, DateTime? ClosedAt);

