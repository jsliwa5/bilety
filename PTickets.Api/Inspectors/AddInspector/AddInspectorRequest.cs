namespace PTickets.Api.Inspectors.AddInspector;

public record AddInspectorRequest(
    string Name,
    string? AssignedToZoneId
);
