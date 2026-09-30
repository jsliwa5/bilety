namespace PTickets.Modules.Tickets;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Tickets.Application.Services;
using PTickets.Modules.Tickets.Contracts;
using PTickets.Modules.Tickets.Domain;
using PTickets.Modules.Tickets.Infrastructure.Endpoints;
using PTickets.Modules.Tickets.Infrastructure.Persistence;
using PTickets.Modules.Tickets.Infrastructure.Providers;

public static class TicketsModule
{
    public static IServiceCollection AddTicketsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TicketsConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=ptickets.db";
        services.AddDbContext<TicketsDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<ITicketRepository, EfTicketRepository>();
        services.AddScoped<IResidentCardRepository, EfResidentCardRepository>();
        services.AddScoped<IStreetZoneMappingRepository, EfStreetZoneMappingRepository>();
        services.AddScoped<ITicketProvider, MockTicketProvider>();
        services.AddScoped<TicketProviderRegistry>();
        services.AddScoped<TicketVerificationService>();
        services.AddScoped<ITicketsModule, TicketsModuleFacade>();
        services.AddHostedService<PTickets.Modules.Tickets.Infrastructure.Messaging.RabbitMqTicketConsumer>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(TicketVerificationService).Assembly,
            typeof(TicketsModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapTicketsEndpoints(this IEndpointRouteBuilder app)
    {
        TicketsEndpoints.MapTicketsEndpoints(app);
        ResidentCardEndpoints.MapResidentCardEndpoints(app);
        return app;
    }
}

