namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class EmptyExclusionReasonException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public EmptyExclusionReasonException() : base("Powód wyłączenia nie może być pusty.") { }

    public EmptyExclusionReasonException(string message) : base(message) { }

    public EmptyExclusionReasonException(string message, System.Exception innerException) : base(message, innerException) { }
}

