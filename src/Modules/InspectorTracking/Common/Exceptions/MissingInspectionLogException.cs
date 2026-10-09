namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class MissingInspectionLogException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public MissingInspectionLogException() : base("Log inspekcji nie może być pusty.") { }

    public MissingInspectionLogException(string message) : base(message) { }

    public MissingInspectionLogException(string message, System.Exception innerException) : base(message, innerException) { }
}
