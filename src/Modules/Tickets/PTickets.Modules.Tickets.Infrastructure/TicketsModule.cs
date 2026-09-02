namespace PTickets.Modules.Tickets;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Tickets.Application.Services;
using PTickets.Modules.Tickets.Domain;
using PTickets.Modules.Tickets.Infrastructure.Endpoints;
using PTickets.Modules.Tickets.Infrastructure.Persistence;
using PTickets.Modules.Tickets.Infrastructure.Providers;

public static class TicketsModule
{
    public static IServiceCollection AddTicketsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketsDbContext>();
        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<ITicketProvider, MockTicketProvider>();
        services.AddScoped<TicketProviderRegistry>();
        services.AddScoped<TicketVerificationService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(TicketVerificationService).Assembly,
            typeof(TicketsModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapTicketsEndpoints(this IEndpointRouteBuilder app)
    {
        TicketsEndpoints.MapTicketsEndpoints(app);
        return app;
    }
}

