namespace PTickets.Modules.Violations.Common.Exceptions;

using PTickets.Shared.Exceptions;

public class InvalidSurchargeTierAmountException : CustomException
{
    public InvalidSurchargeTierAmountException() : base("Amount must be greater than 0.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.BadRequest;
}
