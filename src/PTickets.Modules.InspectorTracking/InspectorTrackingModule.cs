namespace PTickets.Modules.InspectorTracking;
 
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.InspectorTracking.AddInspector;
using PTickets.Modules.InspectorTracking.Data;

public static class InspectorTrackingModule
{
    public static IServiceCollection AddInspectorTrackingModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<InspectorTrackingDbContext>((sp, options) =>
        {
            if (!options.IsConfigured)
            {
                var connectionString = config.GetConnectionString("InspectorTrackingConnection")
                    ?? config.GetConnectionString("DefaultConnection")
                    ?? config.GetConnectionString("Database")
                    ?? "Data Source=ptickets.db";
            }
        });
        return services;
    }

    public static IEndpointRouteBuilder MapInspectorTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAddInspector();
        return app;
    }
}

