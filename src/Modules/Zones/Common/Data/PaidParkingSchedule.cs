namespace PTickets.Modules.Zones.Common.Data;

using PTickets.Modules.Zones.Common.Exceptions;

public record PaidParkingSchedule
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DayOfWeek[] PaidDays { get; init; } = [];

    private PaidParkingSchedule() { } // EF Core

    public PaidParkingSchedule(TimeOnly startTime, TimeOnly endTime, DayOfWeek[]? paidDays)
    {
        if (startTime >= endTime)
            throw new InvalidParkingScheduleException("Czas rozpoczęcia musi być wcześniejszy niż czas zakończenia.");

        if (paidDays is null || paidDays.Length == 0)
            throw new InvalidParkingScheduleException("Harmonogram musi zawierać co najmniej jeden płatny dzień tygodnia.");

        if (paidDays.Any(d => !Enum.IsDefined(typeof(DayOfWeek), d)))
            throw new InvalidParkingScheduleException("Harmonogram zawiera nieprawidłowy dzień tygodnia.");

        StartTime = startTime;
        EndTime = endTime;
        PaidDays = paidDays.Distinct().ToArray();
    }

    public bool IsWithinTimeRange(TimeOnly time) => time >= StartTime && time <= EndTime;

    public bool IsPaidDay(DayOfWeek day) => PaidDays.Contains(day);

    public bool IsPaidAt(DateTime dateTime)
    {
        return IsPaidDay(dateTime.DayOfWeek) && IsWithinTimeRange(TimeOnly.FromDateTime(dateTime));
    }
}

