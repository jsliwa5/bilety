namespace PTickets.Modules.Zones.CreateZone;

using PTickets.Modules.Zones.Data;

public record CreateZoneRequest(
    string Name, 
    ZoneType Type,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DayOfWeek[] PaidDays);
