namespace PTickets.Modules.Zones.Application.Dtos;

using PTickets.Modules.Zones.Domain;

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
