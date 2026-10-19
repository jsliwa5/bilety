namespace PTickets.Modules.Violations.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class DuplicateViolationTypeNameException : CustomException
{
    public DuplicateViolationTypeNameException(string name)
        : base($"Violation type with name '{name}' already exists.")
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}

