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

    public static Street Create(ZoneId zoneId, string name, bool representsWholeZone = false, PaidParkingSchedule? schedule = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa ulicy nie może być pusta.", nameof(name));

        return new Street
        {
            Id = StreetId.New(),
            ZoneId = zoneId,
            Name = name.Trim(),
            RepresentsWholeZone = representsWholeZone,
            PaidParkingSchedule = schedule
        };
    }

    public bool IsPaidAt(DateTime dateTime) => PaidParkingSchedule?.IsPaidAt(dateTime) ?? false;
}
