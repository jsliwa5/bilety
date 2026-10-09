namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InspectionNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public InspectionNotFoundException() : base("InspectionNotFoundException")
    {
    }

    public InspectionNotFoundException(string message) : base(message)
    {
    }
}
