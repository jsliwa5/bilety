namespace PTickets.Modules.Zones.Domain;

using PTickets.Shared;

public class ZoneExclusion
{
    public Guid Id { get; private set; }
    public ZoneId ZoneId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private ZoneExclusion() { } // EF Core

    public static ZoneExclusion Create(ZoneId zoneId, DateTime startDate, DateTime endDate, string reason)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Data początkowa musi być wcześniejsza niż data końcowa.", nameof(startDate));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Powód wyłączenia nie może być pusty.", nameof(reason));

        return new ZoneExclusion
        {
            Id = Guid.NewGuid(),
            ZoneId = zoneId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason.Trim()
        };
    }

    public bool IsActiveAt(DateTime dt) => dt >= StartDate && dt <= EndDate;
}
