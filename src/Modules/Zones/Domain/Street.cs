namespace PTickets.Modules.Zones.Domain;

using PTickets.Shared;

public class Street
{
    public StreetId Id { get; private set; }
    public ZoneId ZoneId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool RepresentsWholeZone { get; private set; }
    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }

    private Street() { } // EF Core

    public static Street CreateNormalStreet(ZoneId zoneId, string name, PaidParkingSchedule? schedule = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa ulicy nie może być pusta.", nameof(name));

        return new Street
        {
            Id = StreetId.New(),
            ZoneId = zoneId,
            Name = name.Trim(),
            RepresentsWholeZone = false,
            PaidParkingSchedule = schedule
        };
    }

    public static Street CreateZoneRepresentative(ZoneId zoneId, string zoneName)
    {
        if (string.IsNullOrWhiteSpace(zoneName))
            throw new ArgumentException("Nazwa strefy nie może być pusta.", nameof(zoneName));

        return new Street
        {
            Id = StreetId.New(),
            ZoneId = zoneId,
            Name = zoneName.Trim(),
            RepresentsWholeZone = true,
            PaidParkingSchedule = null
        };
    }

    public bool IsPaidAt(DateTime dateTime) => PaidParkingSchedule?.IsPaidAt(dateTime) ?? false;
}
