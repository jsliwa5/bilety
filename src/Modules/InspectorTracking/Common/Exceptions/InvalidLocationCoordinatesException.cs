namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidLocationCoordinatesException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidLocationCoordinatesException()
        : base("Współrzędne geograficzne są nieprawidłowe (Latitude: [-90, 90], Longitude: [-180, 180]).")
    {
    }
}

