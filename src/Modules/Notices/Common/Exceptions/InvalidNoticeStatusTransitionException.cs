namespace PTickets.Modules.Notices.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidNoticeStatusTransitionException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public InvalidNoticeStatusTransitionException(string message) : base(message) { }
}