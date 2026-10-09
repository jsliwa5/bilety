namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class SessionAlreadyClosedException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public SessionAlreadyClosedException() : base("SessionAlreadyClosedException")
    {
    }

    public SessionAlreadyClosedException(string message) : base(message)
    {
    }
}
