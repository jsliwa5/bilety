namespace PTickets.Modules.Zones.Domain;

using PTickets.Shared;

public class Zone
{
    public ZoneId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public List<Street> Streets { get; private set; } = [];
    public List<ZoneExclusion> Exclusions { get; private set; } = [];

    private Zone() { } // EF Core

    public static Zone Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa strefy nie może być pusta.", nameof(name));

        return new Zone { Id = ZoneId.New(), Name = name.Trim() };
    }
}
