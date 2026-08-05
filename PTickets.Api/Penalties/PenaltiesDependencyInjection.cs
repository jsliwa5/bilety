using PTickets.Api.Penalties.Data;
using PTickets.Api.Penalties.Infrastructure;
using PTickets.Api.Zones.Contract;
using PTickets.Api.Zones.Infrastructure;

namespace PTickets.Api.Penalties;

public static class PenaltiesDependencyInjection
{
    public static IServiceCollection AddPenaltiesServices(this IServiceCollection services)
    {
        services.AddScoped<IPenaltyRepository, EfPenaltyRepository>();

        return services;
    }
}
