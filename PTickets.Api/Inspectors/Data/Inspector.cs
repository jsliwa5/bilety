using PTickets.Api.Common;

namespace PTickets.Api.Inspectors.Data;

public class Inspector
{
    public InspectorId Id { get; init; }
    public string? Name { get; init; }
    public ZoneId? AssignedToZone { get; set; }
}
