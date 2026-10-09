namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class NoPhotosProvidedException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public NoPhotosProvidedException() : base("NoPhotosProvidedException")
    {
    }

    public NoPhotosProvidedException(string message) : base(message)
    {
    }
}
