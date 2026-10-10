namespace PTickets.Modules.InspectorTracking;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.InspectorTracking.AddInspector;
using PTickets.Modules.InspectorTracking.AssignToZone;
using PTickets.Modules.InspectorTracking.Contracts;
using PTickets.Modules.InspectorTracking.GetAllInspectors;

public static class InspectorTrackingModule
{
    public static IServiceCollection AddInspectorTrackingModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("InspectorTrackingConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=tickets_db;Username=tickets_user;Password=tickets_password";
        services.AddDbContext<InspectorTrackingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IInspectorTrackingModule, InspectorTrackingModuleFacade>();

        return services;
    }

    public static IEndpointRouteBuilder MapInspectorTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAddInspector();
        app.MapAssignToZone();
        app.MapGetAllInspectors();
        return app;
    }
}

