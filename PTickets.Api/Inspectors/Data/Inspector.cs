using PTickets.Api.Common;

namespace PTickets.Api.Inspectors.Data;

public class Inspector
{
    public InspectorId Id { get; init; }
    public string? Name { get; init; }
    public ZoneId? AssignedToZone { get; set; }

    //for EF Core
    private Inspector() { }


    public static Inspector Create(string? name, ZoneId? assignedToZoneId)
    {
        return new Inspector
        {
            Id = new InspectorId(Guid.NewGuid()),
            Name = name,
            AssignedToZone = assignedToZoneId
        };
    }
}
