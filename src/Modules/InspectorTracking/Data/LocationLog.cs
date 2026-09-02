using PTickets.Shared;

namespace PTickets.Modules.InspectorTracking.Data;

internal record LocationLog
{
    public LocationLogId Id { get; init; }
    public DateTime TimeOfLocation { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public LocationLog(InspectorId inspectorId, DateTime timeOfLocation, double latitude, double longitude)
    {
        Id = LocationLogId.New();
        TimeOfLocation = timeOfLocation;
        Latitude = latitude;
        Longitude = longitude;
    }
}
