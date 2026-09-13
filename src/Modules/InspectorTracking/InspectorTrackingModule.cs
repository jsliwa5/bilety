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
        var connectionString = config.GetConnectionString("InspectorTrackingConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Data Source=ptickets.db";
        services.AddDbContext<InspectorTrackingDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }

    public static IEndpointRouteBuilder MapInspectorTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAddInspector();
        app.MapAssignToZone();
        return app;
    }
}

