namespace ExternalNoticesSystem.Api.Features.Chaos;

using ExternalNoticesSystem.Api.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public static class ChaosEndpoint
{
    public static void MapChaosEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chaos/fail-next-requests", ([FromQuery] int count, ChaosState chaos) =>
        {
            chaos.SetFailures(count);
            return Results.Ok(new
            {
                message = $"Następne {count} żądania do /api/external/notices/batch zwrócą 503 Service Unavailable."
            });
        })
        .WithTags("0. Testowanie Polly (Chaos Engineering)");
    }
}

