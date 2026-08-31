namespace PTickets.Modules.Zones.Application.Dtos;

public record ZoneResponse(
    Guid Id,
    string Name,
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
