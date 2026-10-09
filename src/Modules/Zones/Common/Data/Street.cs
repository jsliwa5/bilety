namespace PTickets.Modules.Zones.Common.Data;

using PTickets.Modules.Zones.Common.Exceptions;
using PTickets.Shared;

public class Street
{
    public StreetId Id { get; private set; }
    public ZoneId ZoneId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool RepresentsWholeZone { get; private set; }
    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }

    private Street() { } // EF Core

    public static Street CreateNormalStreet(ZoneId zoneId, string name, PaidParkingSchedule? schedule = null, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidStreetNameException();

        return new Street
        {
            Id = id.HasValue ? new StreetId(id.Value) : StreetId.New(),
            ZoneId = zoneId,
            Name = name.Trim(),
            RepresentsWholeZone = false,
            PaidParkingSchedule = schedule
        };
    }

    public static Street CreateZoneRepresentative(ZoneId zoneId, string zoneName, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(zoneName))
            throw new InvalidZoneNameException();

        return new Street
        {
            Id = id.HasValue ? new StreetId(id.Value) : StreetId.New(),
            ZoneId = zoneId,
            Name = zoneName.Trim(),
            RepresentsWholeZone = true,
            PaidParkingSchedule = null
        };
    }

    public bool IsPaidAt(DateTime dateTime) => PaidParkingSchedule?.IsPaidAt(dateTime) ?? false;
}

