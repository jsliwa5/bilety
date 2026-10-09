namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class OpenSessionAlreadyExistsException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public OpenSessionAlreadyExistsException() : base("OpenSessionAlreadyExistsException")
    {
    }

    public OpenSessionAlreadyExistsException(string message) : base(message)
    {
    }
}
