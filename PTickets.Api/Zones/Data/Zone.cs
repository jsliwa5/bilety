using PTickets.Api.Common;
using PTickets.Api.Zones.Data;


public sealed class Zone
{
    public ZoneId Id { get; }

    public string Name { get; private set; }

    private Zone()
    {
    }

    public Zone(
        ZoneId id,
        string name)
    {
        Id = id;
        Name = name;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}