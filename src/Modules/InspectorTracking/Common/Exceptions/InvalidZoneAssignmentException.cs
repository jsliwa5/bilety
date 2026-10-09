namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidZoneAssignmentException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidZoneAssignmentException() : base("Podana strefa nie istnieje.") { }

    public InvalidZoneAssignmentException(string message) : base(message) { }

    public InvalidZoneAssignmentException(string message, System.Exception innerException) : base(message, innerException) { }
}
