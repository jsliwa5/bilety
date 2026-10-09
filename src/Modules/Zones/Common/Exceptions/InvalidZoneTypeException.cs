namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidZoneTypeException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidZoneTypeException() : base("Nieprawidłowy typ strefy.") { }

    public InvalidZoneTypeException(string message) : base(message) { }

    public InvalidZoneTypeException(string message, System.Exception innerException) : base(message, innerException) { }
}

