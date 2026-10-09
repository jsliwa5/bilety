namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class MissingLocationLogException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public MissingLocationLogException() : base("Log lokalizacji nie może być pusty.") { }

    public MissingLocationLogException(string message) : base(message) { }

    public MissingLocationLogException(string message, System.Exception innerException) : base(message, innerException) { }
}
