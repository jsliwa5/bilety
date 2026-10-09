namespace PTickets.Modules.Notices.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class CannotPayNoticeException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public CannotPayNoticeException() : base("Nie można opłacić wezwania, które nie ma statusu wydanego (Issued).") { }

    public CannotPayNoticeException(string message) : base(message) { }

    public CannotPayNoticeException(string message, System.Exception innerException) : base(message, innerException) { }
}
