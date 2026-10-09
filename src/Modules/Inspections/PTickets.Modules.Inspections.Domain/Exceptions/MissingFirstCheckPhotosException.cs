namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class MissingFirstCheckPhotosException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public MissingFirstCheckPhotosException() : base("MissingFirstCheckPhotosException")
    {
    }

    public MissingFirstCheckPhotosException(string message) : base(message)
    {
    }
}
