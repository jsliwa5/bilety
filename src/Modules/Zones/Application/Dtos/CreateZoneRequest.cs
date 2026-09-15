namespace PTickets.Modules.Zones.Application.Dtos;

using PTickets.Modules.Zones.Domain;

public record CreateZoneRequest(
    string Name, 
    ZoneType Type,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    DayOfWeek[]? PaidDays = null);
