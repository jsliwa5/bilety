namespace PTickets.Modules.Zones.Common.Exceptions;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidParkingScheduleException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidParkingScheduleException() : base("Czas rozpoczęcia musi być wcześniejszy niż czas zakończenia.") { }

    public InvalidParkingScheduleException(string message) : base(message) { }

    public InvalidParkingScheduleException(string message, System.Exception innerException) : base(message, innerException) { }
}

