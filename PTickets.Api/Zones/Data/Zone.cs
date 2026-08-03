using PTickets.Api.Common;
using PTickets.Api.Zones.Data;


public class Zone
{
    public ZoneId Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<Street> _streets = new();
    public IReadOnlyCollection<Street> Streets => _streets.AsReadOnly();

    public bool HasStreets => _streets.Count > 0;

    public Zone(ZoneId id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddStreet(Street street)
    {
        if (_streets.Any(s => s.Id == street.Id))
            throw new InvalidOperationException("Ulica o tym ID już istnieje w strefie.");

        _streets.Add(street);
    }

    public bool ContainsStreet(StreetId streetId)
        => _streets.Any(s => s.Id == streetId);
}