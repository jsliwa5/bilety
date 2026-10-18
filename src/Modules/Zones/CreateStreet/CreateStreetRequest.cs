namespace PTickets.Modules.Zones.CreateStreet;

public record CreateStreetRequest(
    string Name,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    DayOfWeek[]? PaidDays = null);

