namespace PTickets.Modules.Zones.Domain;

public record PaidParkingSchedule
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DayOfWeek[] PaidDays { get; init; } = [];

    private PaidParkingSchedule() { } // EF Core

    public PaidParkingSchedule(TimeOnly startTime, TimeOnly endTime, DayOfWeek[]? paidDays)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Czas rozpoczęcia musi być wcześniejszy niż czas zakończenia.", nameof(startTime));

        StartTime = startTime;
        EndTime = endTime;
        PaidDays = paidDays ?? [];
    }

    public bool IsWithinTimeRange(TimeOnly time) => time >= StartTime && time <= EndTime;

    public bool IsPaidDay(DayOfWeek day) => PaidDays.Contains(day);

    public bool IsPaidAt(DateTime dateTime)
    {
        return IsPaidDay(dateTime.DayOfWeek) && IsWithinTimeRange(TimeOnly.FromDateTime(dateTime));
    }
}
