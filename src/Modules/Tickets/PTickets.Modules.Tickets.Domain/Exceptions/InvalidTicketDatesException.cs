namespace PTickets.Modules.Tickets.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidTicketDatesException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidTicketDatesException()
        : base("Ticket ValidTo date must be greater than ValidFrom date.")
    {
    }
}

