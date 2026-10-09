namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidInspectorFirstNameException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidInspectorFirstNameException() : base("Imię inspektora nie może być puste.") { }

    public InvalidInspectorFirstNameException(string message) : base(message) { }

    public InvalidInspectorFirstNameException(string message, System.Exception innerException) : base(message, innerException) { }
}
