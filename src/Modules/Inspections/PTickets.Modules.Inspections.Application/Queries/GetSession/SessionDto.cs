namespace PTickets.Modules.Inspections.Application.Queries.GetSession;

public record SessionDto(Guid Id, Guid InspectorId, DateTime StartedAt, DateTime? ClosedAt);

