namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class NoViolationsForNoticeException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public NoViolationsForNoticeException() : base("NoViolationsForNoticeException")
    {
    }

    public NoViolationsForNoticeException(string message) : base(message)
    {
    }
}
