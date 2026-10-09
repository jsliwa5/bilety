namespace PTickets.Shared.Exceptions;

using System;
using System.Net;

public abstract class CustomException : Exception
{
    public abstract HttpStatusCode StatusCode { get; }

    protected CustomException() : base() { }

    protected CustomException(string message) : base(message) { }

    protected CustomException(string message, Exception innerException) : base(message, innerException) { }
}

