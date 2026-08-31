using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace PTickets.Modules.Notices;

public static class NoticesModule
{
    public static IServiceCollection AddNoticesModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapNoticesEndpoints(this IEndpointRouteBuilder app)
    {
        return app;
    }
}

