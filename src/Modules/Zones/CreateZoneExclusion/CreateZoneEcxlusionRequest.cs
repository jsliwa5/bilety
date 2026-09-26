namespace PTickets.Modules.Zones.CreateZoneExclusion;

public record CreateZoneExclusionRequest(DateTime StartDate, DateTime EndDate, string Reason);
