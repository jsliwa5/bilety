namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class ViolationTypeNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public ViolationTypeNotFoundException() : base("ViolationTypeNotFoundException")
    {
    }

    public ViolationTypeNotFoundException(string message) : base(message)
    {
    }
}
