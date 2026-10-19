namespace PTickets.Modules.Tickets.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidTicketDataException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidTicketDataException(string message) : base(message)
    {
    }
}

