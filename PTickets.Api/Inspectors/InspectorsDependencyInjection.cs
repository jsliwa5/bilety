using Microsoft.Extensions.DependencyInjection;
using PTickets.Api.Inspectors.Contracts;
using PTickets.Api.Inspectors.Data;
using PTickets.Api.Inspectors.Infrastructure;

namespace PTickets.Api.Inspectors;

public static class InspectorsDependencyInjection
{
    public static IServiceCollection AddInspectorsServices(this IServiceCollection services)
    {
        services.AddScoped<IInspectorRepository, EfInspectorRepository>();
        services.AddScoped<IInspectorFacade, InspectorFacade>();

        return services;
    }
}
