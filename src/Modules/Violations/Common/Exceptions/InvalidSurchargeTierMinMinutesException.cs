namespace PTickets.Modules.Violations.Common.Exceptions;

using PTickets.Shared.Exceptions;

public class InvalidSurchargeTierMinMinutesException : CustomException
{
    public InvalidSurchargeTierMinMinutesException() : base("MinMinutes must be greater than or equal to 0.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.BadRequest;
}
