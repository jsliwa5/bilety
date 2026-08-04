namespace PTickets.Api.Zones.AddStreet;

public record AddStreetRequest(
    string Name,
    string ZoneId,
    string? StartTime,
    string? EndTime,
    string? PaidDays
);
