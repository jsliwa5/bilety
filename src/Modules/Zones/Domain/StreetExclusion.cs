namespace PTickets.Modules.Zones.Domain;

using PTickets.Shared;

public class StreetExclusion
{
    public Guid Id { get; private set; }
    public StreetId StreetId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private StreetExclusion() { } // EF Core

    public static StreetExclusion Create(StreetId streetId, DateTime startDate, DateTime endDate, string reason)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Data początkowa musi być wcześniejsza niż data końcowa.", nameof(startDate));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Powód wyłączenia nie może być pusty.", nameof(reason));

        return new StreetExclusion
        {
            Id = Guid.NewGuid(),
            StreetId = streetId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason.Trim()
        };
    }

    public bool IsActiveAt(DateTime dt) => dt >= StartDate && dt <= EndDate;
}
