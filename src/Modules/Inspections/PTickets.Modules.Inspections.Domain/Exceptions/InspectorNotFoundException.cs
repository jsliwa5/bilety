namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InspectorNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public InspectorNotFoundException() : base("InspectorNotFoundException")
    {
    }

    public InspectorNotFoundException(string message) : base(message)
    {
    }
}
