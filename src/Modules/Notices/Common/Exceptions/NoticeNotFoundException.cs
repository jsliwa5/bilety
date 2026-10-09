namespace PTickets.Modules.Notices.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class NoticeNotFoundException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public NoticeNotFoundException() : base("Nie znaleziono wezwania do zapłaty.") { }

    public NoticeNotFoundException(string message) : base(message) { }

    public NoticeNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}
