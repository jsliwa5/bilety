namespace PTickets.Modules.Inspections.Domain;

using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public class Inspection
{
    public InspectionId Id { get; private set; }
    public InspectorId InspectorId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime StartedAt { get; private set; }

    public Inspection(InspectionId id, InspectorId inspectorId, double latitude, double longitude, DateTime startedAt)
    {
        Id = id;
        InspectorId = inspectorId;
        Latitude = latitude;
        Longitude = longitude;
        StartedAt = startedAt;
    }
}

