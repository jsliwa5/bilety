namespace PTickets.Modules.InspectorTracking.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InspectorNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public InspectorNotFoundException() : base("Inspektor nie został znaleziony.") { }

    public InspectorNotFoundException(string message) : base(message) { }

    public InspectorNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}
