namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class ZoneNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public ZoneNotFoundException() : base("Nie znaleziono strefy.") { }

    public ZoneNotFoundException(string message) : base(message) { }

    public ZoneNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}

