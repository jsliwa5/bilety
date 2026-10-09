namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class NoticeAlreadyIssuedTodayException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public NoticeAlreadyIssuedTodayException() : base("NoticeAlreadyIssuedTodayException")
    {
    }

    public NoticeAlreadyIssuedTodayException(string message) : base(message)
    {
    }
}
