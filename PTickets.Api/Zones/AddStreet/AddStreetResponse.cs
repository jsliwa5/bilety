using PTickets.Api.Common;

namespace PTickets.Api.Zones.AddStreet;

public record AddStreetResponse(
    StreetId Id,
    string Name
);
