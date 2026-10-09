namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidInspectorLastNameException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidInspectorLastNameException() : base("Nazwisko inspektora nie może być puste.") { }

    public InvalidInspectorLastNameException(string message) : base(message) { }

    public InvalidInspectorLastNameException(string message, System.Exception innerException) : base(message, innerException) { }
}
