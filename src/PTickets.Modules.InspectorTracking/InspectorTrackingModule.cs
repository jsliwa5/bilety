namespace PTickets.Modules.InspectorTracking;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.InspectorTracking.AddInspector;
using PTickets.Modules.InspectorTracking.AssignToZone;

public static class InspectorTrackingModule
{
    public static IServiceCollection AddInspectorTrackingModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<InspectorTrackingDbContext>(options =>
        {
            // Provider konfigurowany w PTickets.Api (SQLite/PostgreSQL)
        });

        return services;
    }

    public static IEndpointRouteBuilder MapInspectorTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAddInspector();
        app.MapAssignToZone();
        return app;
    }
}

