namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class SessionNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public SessionNotFoundException() : base("SessionNotFoundException")
    {
    }

    public SessionNotFoundException(string message) : base(message)
    {
    }
}
