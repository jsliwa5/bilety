using Microsoft.Extensions.DependencyInjection;
using PTickets.Api.Zones.Contract;
using PTickets.Api.Zones.Data;
using PTickets.Api.Zones.Infrastructure;

namespace PTickets.Api.Zones;

public static class ZonesDependencyInjection
{
    public static IServiceCollection AddZonesServices(this IServiceCollection services)
    {
        services.AddScoped<IZoneRepository, EfZoneRepository>();
        services.AddScoped<IZoneFacade, ZoneFacade>();

        return services;
    }
}
