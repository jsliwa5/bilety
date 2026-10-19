namespace PTickets.Modules.Inspections.Domain.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class CannotAddNoTicketManuallyException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public CannotAddNoTicketManuallyException()
        : base("Wykroczenie braku biletu (NoTicket) nie może zostać dodane ręcznie jako wykroczenie wizualne.")
    {
    }
}

