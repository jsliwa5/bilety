using Microsoft.Extensions.DependencyInjection;
using PTickets.Api.Inspections.Contracts;
using PTickets.Api.Inspections.Data;
using PTickets.Api.Inspections.Infrastructure;

namespace PTickets.Api.Inspections;

public static class InspectionsDependencyInjection
{
    public static IServiceCollection AddInspectionsServices(this IServiceCollection services)
    {
        services.AddScoped<IInspectionRepository, EfInspectionRepository>();
        services.AddSingleton<ITicketProviderClient, RandomTicketProviderClient>();

        return services;
    }
}
