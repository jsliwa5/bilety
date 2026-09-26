namespace PTickets.Modules.Zones.Data;

using PTickets.Shared;

public class Zone
{
    private readonly List<Street> _streets = [];
    private readonly List<ZoneExclusion> _exclusions = [];

    public ZoneId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ZoneType Type { get; private set; }
    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }

    public IReadOnlyCollection<Street> Streets => _streets.AsReadOnly();
    public IReadOnlyCollection<ZoneExclusion> Exclusions => _exclusions.AsReadOnly();

    private Zone() { } // EF Core

    public static Zone CreateSingle(string name, PaidParkingSchedule? schedule = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa strefy nie może być pusta.", nameof(name));

        var zone = new Zone
        {
            Id = ZoneId.New(),
            Name = name.Trim(),
            Type = ZoneType.Single,
            PaidParkingSchedule = schedule
        };

        zone._streets.Add(Street.CreateZoneRepresentative(zone.Id, zone.Name));
        return zone;
    }

    public static Zone CreateMultiStreet(string name, PaidParkingSchedule? schedule = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa strefy nie może być pusta.", nameof(name));

        return new Zone
        {
            Id = ZoneId.New(),
            Name = name.Trim(),
            Type = ZoneType.MultiStreet,
            PaidParkingSchedule = schedule
        };
    }

    public static Zone Create(string name, ZoneType type, PaidParkingSchedule? schedule = null) =>
        type switch
        {
            ZoneType.Single => CreateSingle(name, schedule),
            ZoneType.MultiStreet => CreateMultiStreet(name, schedule),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Nieznany typ strefy.")
        };

    public Street AddStreet(string name, PaidParkingSchedule? schedule = null)
    {
        if (Type == ZoneType.Single)
            throw new InvalidOperationException("Nie można dodawać nowych ulic do pojedynczej strefy (Single).");

        var street = Street.CreateNormalStreet(Id, name, schedule);
        _streets.Add(street);
        return street;
    }

    public ZoneExclusion AddExclusion(DateTime startDate, DateTime endDate, string reason)
    {
        var exclusion = ZoneExclusion.Create(Id, startDate, endDate, reason);
        _exclusions.Add(exclusion);
        return exclusion;
    }
}

