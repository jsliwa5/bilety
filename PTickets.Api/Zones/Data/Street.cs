

using PTickets.Api.Common;

namespace PTickets.Api.Zones.Data;

public class Street
{
    public StreetId Id { get; private set; }
    public string Name { get; private set; }

    public Street(StreetId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa ulicy nie może być pusta.");

        Id = id;
        Name = name;
    }
}