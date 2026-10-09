namespace PTickets.Modules.Violations.Common.Exceptions;

using System;
using PTickets.Shared.Exceptions;

public class ViolationTypeNotFoundException : CustomException
{
    public ViolationTypeNotFoundException(Guid id) : base("Violation type with ID {id} was not found.")
    {
    }
    public override System.Net.HttpStatusCode StatusCode => System.Net.HttpStatusCode.NotFound;
}
