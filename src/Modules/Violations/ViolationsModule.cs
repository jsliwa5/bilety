namespace PTickets.Modules.Violations;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Violations.Application.Services;
using PTickets.Modules.Violations.Endpoints;
using PTickets.Modules.Violations.Infrastructure.Persistence;

public static class ViolationsModule
{
    public static IServiceCollection AddViolationsModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ViolationsConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? "Data Source=ptickets.db";
        services.AddDbContext<ViolationsDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<PenaltyCalculationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapViolationsEndpoints(this IEndpointRouteBuilder app)
    {
        ViolationsEndpoints.MapViolationsEndpoints(app);
        return app;
    }
}
