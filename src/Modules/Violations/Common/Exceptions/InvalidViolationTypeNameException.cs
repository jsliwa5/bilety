namespace PTickets.Modules.Violations.Common.Exceptions;

using PTickets.Shared.Exceptions;

public class InvalidViolationTypeNameException : CustomException
{
    public InvalidViolationTypeNameException() : base("Violation type name cannot be null or empty.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.BadRequest;
}
