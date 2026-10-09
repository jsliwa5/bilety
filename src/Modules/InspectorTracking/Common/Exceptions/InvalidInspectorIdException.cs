namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidInspectorIdException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidInspectorIdException() : base("Id inspektora nie może być puste.") { }

    public InvalidInspectorIdException(string message) : base(message) { }

    public InvalidInspectorIdException(string message, System.Exception innerException) : base(message, innerException) { }
}
