namespace PTickets.Modules.Notifications;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Notifications.Endpoints;
using PTickets.Modules.Notifications.Infrastructure.External;
using PTickets.Modules.Notifications.Infrastructure.Persistence;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Notifications")
            ?? configuration.GetConnectionString("NotificationsModule")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=ptickets.db";

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<ISmsGateway, MockSmsGateway>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotificationsModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapNotificationsApiEndpoints();
        return app;
    }
}

