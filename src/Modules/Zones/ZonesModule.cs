namespace PTickets.Modules.Zones;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Zones.Application.Services;
using PTickets.Modules.Zones.Endpoints;
using PTickets.Modules.Zones.Infrastructure.Persistence;

public static class ZonesModule
{
    public static IServiceCollection AddZonesModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ZonesDbContext>((sp, options) =>
        {
            if (!options.IsConfigured)
            {
                var connectionString = config.GetConnectionString("ZonesConnection")
                    ?? config.GetConnectionString("DefaultConnection")
                    ?? config.GetConnectionString("Database")
                    ?? "Data Source=ptickets.db";
            }
        });

        services.AddScoped<ZoneManagementService>();

        return services;
    }

    public static IEndpointRouteBuilder MapZonesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapZonesApiEndpoints();
        app.MapExclusionsApiEndpoints();
        return app;
    }
}
