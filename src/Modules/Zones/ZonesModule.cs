namespace PTickets.Modules.Zones;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Zones.Contracts;
using PTickets.Modules.Zones.CreateStreet;
using PTickets.Modules.Zones.CreateStreetExclusion;
using PTickets.Modules.Zones.CreateZone;
using PTickets.Modules.Zones.CreateZoneExclusion;
using PTickets.Modules.Zones.GetAllZones;

public static class ZonesModule
{
    public static IServiceCollection AddZonesModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ZonesConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=tickets_db;Username=tickets_user;Password=tickets_password";
        services.AddDbContext<ZonesDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IZonesModule, ZonesModuleFacade>();

        return services;
    }

    public static IEndpointRouteBuilder MapZonesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateStreetEndpoint();
        app.MapCreateZoneEndpoint();
        app.MapGetAllZonesEndpoint();
        app.MapCreateStreetExclusionEndpoint();
        app.MapCreateZoneExclusionEndpoint();
        return app;
    }
}
