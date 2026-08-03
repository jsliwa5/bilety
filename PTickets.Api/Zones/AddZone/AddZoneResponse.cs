using PTickets.Api.Common;

namespace PTickets.Api.Zones.AddZone;

public record AddZoneResponse(
    ZoneId Id,
    string Name
);
