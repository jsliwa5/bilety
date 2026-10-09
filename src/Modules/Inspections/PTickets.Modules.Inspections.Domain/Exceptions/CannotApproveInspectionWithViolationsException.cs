namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class CannotApproveInspectionWithViolationsException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public CannotApproveInspectionWithViolationsException() : base("CannotApproveInspectionWithViolationsException")
    {
    }

    public CannotApproveInspectionWithViolationsException(string message) : base(message)
    {
    }
}
