namespace PTickets.Modules.Tickets.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidResidentCardDatesException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidResidentCardDatesException() 
        : base("ValidTo must be greater than ValidFrom.")
    {
    }
}
