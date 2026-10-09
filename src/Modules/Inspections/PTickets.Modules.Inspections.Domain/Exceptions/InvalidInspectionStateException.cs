namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidInspectionStateException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public InvalidInspectionStateException() : base("InvalidInspectionStateException")
    {
    }

    public InvalidInspectionStateException(string message) : base(message)
    {
    }
}
