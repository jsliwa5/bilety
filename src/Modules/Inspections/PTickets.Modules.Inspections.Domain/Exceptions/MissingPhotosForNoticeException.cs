namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class MissingPhotosForNoticeException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public MissingPhotosForNoticeException() : base("MissingPhotosForNoticeException")
    {
    }

    public MissingPhotosForNoticeException(string message) : base(message)
    {
    }
}
