namespace PTickets.Modules.Zones.Domain;

using PTickets.Shared;

public class Zone
{
    public ZoneId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ZoneType Type { get; private set; }
    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }
    public List<Street> Streets { get; private set; } = [];
    public List<ZoneExclusion> Exclusions { get; private set; } = [];

    private Zone() { } // EF Core

    public static Zone Create(string name, ZoneType type, PaidParkingSchedule? schedule = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa strefy nie może być pusta.", nameof(name));

        return new Zone 
        { 
            Id = ZoneId.New(), 
            Name = name.Trim(), 
            Type = type,
            PaidParkingSchedule = schedule
        };
    }
}

