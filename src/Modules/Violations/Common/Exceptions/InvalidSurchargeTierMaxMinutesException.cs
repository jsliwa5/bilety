namespace PTickets.Modules.Violations.Common.Exceptions;

using PTickets.Shared.Exceptions;

public class InvalidSurchargeTierMaxMinutesException : CustomException
{
    public InvalidSurchargeTierMaxMinutesException() : base("MaxMinutes must be greater than MinMinutes.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.BadRequest;
}
