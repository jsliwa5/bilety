namespace PTickets.Modules.Zones.Application.Dtos;

public record CreateExclusionRequest(DateTime StartDate, DateTime EndDate, string Reason);
