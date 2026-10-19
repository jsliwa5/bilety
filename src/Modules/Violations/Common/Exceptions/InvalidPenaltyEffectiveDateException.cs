namespace PTickets.Modules.Violations.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidPenaltyEffectiveDateException : CustomException
{
    public InvalidPenaltyEffectiveDateException()
        : base("EffectiveFrom date cannot be earlier than today.")
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}

