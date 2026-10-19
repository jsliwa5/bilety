namespace PTickets.Modules.Notices.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidNoticeAmountException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidNoticeAmountException(string message) : base(message) { }
}

