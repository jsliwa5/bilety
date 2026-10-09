namespace PTickets.Modules.Zones.Common.Data;

using PTickets.Modules.Zones.Common.Exceptions;
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

    public static Zone CreateSingle(string name, PaidParkingSchedule? schedule = null, Guid? id = null, Guid? streetId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidZoneNameException();

        var zone = new Zone
        {
            Id = id.HasValue ? new ZoneId(id.Value) : ZoneId.New(),
            Name = name.Trim(),
            Type = ZoneType.Single,
            PaidParkingSchedule = schedule
        };

        zone._streets.Add(Street.CreateZoneRepresentative(zone.Id, zone.Name, streetId));
        return zone;
    }

    public static Zone CreateMultiStreet(string name, PaidParkingSchedule? schedule = null, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidZoneNameException();

        return new Zone
        {
            Id = id.HasValue ? new ZoneId(id.Value) : ZoneId.New(),
            Name = name.Trim(),
            Type = ZoneType.MultiStreet,
            PaidParkingSchedule = schedule
        };
    }

    public static Zone Create(string name, ZoneType type, PaidParkingSchedule? schedule = null, Guid? id = null, Guid? streetId = null) =>
        type switch
        {
            ZoneType.Single => CreateSingle(name, schedule, id, streetId),
            ZoneType.MultiStreet => CreateMultiStreet(name, schedule, id),
            _ => throw new InvalidZoneTypeException()
        };

    public Street AddStreet(string name, PaidParkingSchedule? schedule = null, Guid? id = null)
    {
        if (Type == ZoneType.Single)
            throw new CannotAddStreetToSingleZoneException();

        var street = Street.CreateNormalStreet(Id, name, schedule, id);
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


