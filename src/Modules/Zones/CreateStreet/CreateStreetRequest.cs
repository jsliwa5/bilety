namespace PTickets.Modules.Zones.CreateStreet;

public record CreateStreetRequest(
    string Name,
    bool RepresentsWholeZone = false,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    DayOfWeek[]? PaidDays = null);
