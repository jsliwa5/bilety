namespace PTickets.Shared.ValueObjects;

using System.Net;
using PTickets.Shared.Exceptions;

public class InvalidRegistrationNumberException : CustomException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public InvalidRegistrationNumberException() 
        : base("Nieprawidłowy numer rejestracyjny.") { }

    public InvalidRegistrationNumberException(string message) 
        : base(message) { }

    public InvalidRegistrationNumberException(string message, Exception innerException) 
        : base(message, innerException) { }
}

