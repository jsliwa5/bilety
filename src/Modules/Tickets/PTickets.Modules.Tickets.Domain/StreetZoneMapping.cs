namespace PTickets.Modules.Tickets.Domain;

using PTickets.Shared;

public class StreetZoneMapping
{
    public StreetId StreetId { get; private set; }
    public ZoneId ZoneId { get; private set; }

    private StreetZoneMapping() { }

    public static StreetZoneMapping Create(StreetId streetId, ZoneId zoneId)
    {
        return new StreetZoneMapping
        {
            StreetId = streetId,
            ZoneId = zoneId
        };
    }
}

