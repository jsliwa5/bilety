namespace PTickets.Modules.Violations.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class OverlappingSurchargeTierException : CustomException
{
    public OverlappingSurchargeTierException()
        : base("The surcharge tier minute range overlaps with an existing tier.")
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}

