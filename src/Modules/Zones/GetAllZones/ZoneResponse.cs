namespace PTickets.Modules.Zones.GetAllZones;

using PTickets.Modules.Zones.Data;

public record ZoneResponse(
    Guid Id,
    string Name,
    ZoneType Type,
    ScheduleResponse? Schedule,
    IReadOnlyList<StreetResponse> Streets);

public record StreetResponse(
    Guid Id,
    string Name,
    bool RepresentsWholeZone,
    ScheduleResponse? Schedule);

public record ScheduleResponse(
    TimeOnly StartTime,
    TimeOnly EndTime,
    DayOfWeek[] PaidDays);
