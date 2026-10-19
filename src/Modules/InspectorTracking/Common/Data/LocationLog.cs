using PTickets.Modules.InspectorTracking.Common.Exceptions;
using PTickets.Shared;

namespace PTickets.Modules.InspectorTracking.Common.Data;

internal record LocationLog
{
    public LocationLogId Id { get; init; }
    public DateTime TimeOfLocation { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    private LocationLog() { } // EF Core

    public LocationLog(InspectorId inspectorId, DateTime timeOfLocation, double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || double.IsInfinity(latitude) || latitude is < -90.0 or > 90.0)
            throw new InvalidLocationCoordinatesException();

        if (double.IsNaN(longitude) || double.IsInfinity(longitude) || longitude is < -180.0 or > 180.0)
            throw new InvalidLocationCoordinatesException();

        Id = LocationLogId.New();
        TimeOfLocation = timeOfLocation;
        Latitude = latitude;
        Longitude = longitude;
    }
}

