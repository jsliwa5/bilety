namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidStreetNameException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidStreetNameException() : base("Nazwa ulicy nie może być pusta.") { }

    public InvalidStreetNameException(string message) : base(message) { }

    public InvalidStreetNameException(string message, System.Exception innerException) : base(message, innerException) { }
}

