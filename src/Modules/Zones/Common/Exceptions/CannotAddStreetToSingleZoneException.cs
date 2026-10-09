namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class CannotAddStreetToSingleZoneException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public CannotAddStreetToSingleZoneException() : base("Nie można dodawać nowych ulic do pojedynczej strefy (Single).") { }

    public CannotAddStreetToSingleZoneException(string message) : base(message) { }

    public CannotAddStreetToSingleZoneException(string message, System.Exception innerException) : base(message, innerException) { }
}

