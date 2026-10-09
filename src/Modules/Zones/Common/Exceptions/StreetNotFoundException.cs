namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class StreetNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public StreetNotFoundException() : base("Nie znaleziono ulicy.") { }

    public StreetNotFoundException(string message) : base(message) { }

    public StreetNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}

