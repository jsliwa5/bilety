namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidExclusionDatesException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidExclusionDatesException() : base("Data początkowa musi być wcześniejsza niż data końcowa.") { }

    public InvalidExclusionDatesException(string message) : base(message) { }

    public InvalidExclusionDatesException(string message, System.Exception innerException) : base(message, innerException) { }
}

