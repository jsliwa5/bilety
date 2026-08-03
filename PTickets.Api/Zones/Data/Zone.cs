using PTickets.Api.Common;
using PTickets.Api.Zones.Data;


public class Zone
{
    public ZoneId Id { get; private set; }
    public string Name { get; private set; }
    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }

    private readonly List<Street> _streets = new();
    public IReadOnlyCollection<Street> Streets => _streets.AsReadOnly();

    public bool HasStreets => _streets.Count > 0;

    // Konstruktor dla EF Core
    private Zone()
    {
        Name = string.Empty;
    }

    public Zone(ZoneId id, string name, PaidParkingSchedule? paidParkingSchedule = null)
    {
        Id = id;
        Name = name;
        PaidParkingSchedule = paidParkingSchedule;
    }

    public void AddStreet(Street street)
    {
        if (_streets.Any(s => s.Id == street.Id))
            throw new InvalidOperationException("Ulica o tym ID już istnieje w strefie.");

        _streets.Add(street);
    }

    public bool ContainsStreet(StreetId streetId)
        => _streets.Any(s => s.Id == streetId);

    public bool IsPaid(DateTime dateTime)
    {
        return PaidParkingSchedule?.IsPaidTime(dateTime) ?? false;
    }
}