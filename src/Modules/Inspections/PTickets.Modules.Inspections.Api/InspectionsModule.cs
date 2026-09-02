using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace PTickets.Modules.Inspections;

public static class InspectionsModule
{
    public static IServiceCollection AddInspectionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapInspectionsEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}

