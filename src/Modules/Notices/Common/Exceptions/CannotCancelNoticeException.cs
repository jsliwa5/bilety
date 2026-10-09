namespace PTickets.Modules.Notices.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class CannotCancelNoticeException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public CannotCancelNoticeException() : base("Nie można anulować wezwania, które nie ma statusu wydanego (Issued).") { }

    public CannotCancelNoticeException(string message) : base(message) { }

    public CannotCancelNoticeException(string message, System.Exception innerException) : base(message, innerException) { }
}
