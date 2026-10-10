namespace PTickets.Api.ExceptionHandling;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PTickets.Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is CustomException customException)
        {
            logger.LogWarning(exception, "Domain/Application exception: {Message}", customException.Message);
            
            httpContext.Response.StatusCode = (int)customException.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = (int)customException.StatusCode,
                Type = customException.GetType().Name,
                Title = "Wystąpił błąd w logice aplikacji",
                Detail = customException.Message,
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true;
        }

        if (exception is BadHttpRequestException badRequestEx)
        {
            logger.LogWarning(exception, "Bad HTTP request: {Message}", badRequestEx.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "BadHttpRequest",
                Title = "Nieprawidłowe żądanie HTTP",
                Detail = badRequestEx.Message,
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled system exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "InternalServerError",
            Title = "Błąd wewnętrzny serwera",
            Detail = "Wystąpił niespodziewany błąd po stronie serwera.",
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}

