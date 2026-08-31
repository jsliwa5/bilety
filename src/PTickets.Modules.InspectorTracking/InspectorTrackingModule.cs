using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace PTickets.Modules.InspectorTracking;

public static class InspectorTrackingModule
{
    public static IServiceCollection AddInspectorTrackingModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapInspectorTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}

