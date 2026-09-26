namespace PTickets.Modules.Zones.CreateStreetExclusion;

public record CreateStreetExclusionRequest(DateTime StartDate, DateTime EndDate, string Reason);
