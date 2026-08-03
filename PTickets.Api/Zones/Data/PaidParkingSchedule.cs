namespace PTickets.Api.Zones.Data;

/// <summary>
/// ValueObject reprezentujący harmonogram płatnego parkowania w strefie.
/// Określa przedziały czasowe i dni tygodnia, w których strefa jest płatna.
/// </summary>
public record PaidParkingSchedule
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DayOfWeek[] PaidDays { get; init; }

    public PaidParkingSchedule(TimeOnly startTime, TimeOnly endTime, DayOfWeek[] paidDays)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Godzina rozpoczęcia musi być wcześniejsza niż godzina zakończenia.");

        if (paidDays == null || paidDays.Length == 0)
            throw new ArgumentException("Nie można określić harmonogramu bez dni tygodnia.");

        StartTime = startTime;
        EndTime = endTime;
        PaidDays = paidDays;
    }

    /// <summary>
    /// Sprawdza, czy podana data i godzina są w ramach okresu płatnego parkowania.
    /// </summary>
    public bool IsPaidTime(DateTime dateTime)
    {
        if (!PaidDays.Contains(dateTime.DayOfWeek))
            return false;

        var currentTime = dateTime.TimeOfDay;
        return currentTime >= StartTime.ToTimeSpan() && currentTime < EndTime.ToTimeSpan();
    }

    /// <summary>
    /// Sprawdza, czy podana godzina jest w ramach przedziału czasowego płatnego parkowania.
    /// </summary>
    public bool IsWithinTimeRange(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }

    /// <summary>
    /// Sprawdza, czy podany dzień jest dniem płatnego parkowania.
    /// </summary>
    public bool IsPaidDay(DayOfWeek dayOfWeek)
    {
        return PaidDays.Contains(dayOfWeek);
    }
}
