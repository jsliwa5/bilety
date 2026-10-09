namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidZoneNameException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidZoneNameException() : base("Nazwa strefy nie może być pusta.") { }

    public InvalidZoneNameException(string message) : base(message) { }

    public InvalidZoneNameException(string message, System.Exception innerException) : base(message, innerException) { }
}

