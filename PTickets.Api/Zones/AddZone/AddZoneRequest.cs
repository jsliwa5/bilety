namespace PTickets.Api.Zones.AddZone;

public record AddZoneRequest(
    string Name,
    string? StartTime,
    string? EndTime,
    string? PaidDays
);
