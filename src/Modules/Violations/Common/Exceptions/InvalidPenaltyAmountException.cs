namespace PTickets.Modules.Violations.Common.Exceptions;

using PTickets.Shared.Exceptions;

public class InvalidPenaltyAmountException : CustomException
{
    public InvalidPenaltyAmountException() : base("Penalty amount must be greater than zero.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.BadRequest;
}
