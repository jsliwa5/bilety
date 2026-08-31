namespace PTickets.Modules.Violations;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Violations.Application.Services;
using PTickets.Modules.Violations.Endpoints;
using PTickets.Modules.Violations.Infrastructure.Persistence;

public static class ViolationsModule
{
    public static IServiceCollection AddViolationsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ViolationsDbContext>();
        services.AddScoped<PenaltyCalculationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapViolationsEndpoints(this IEndpointRouteBuilder app)
    {
        ViolationsEndpoints.MapViolationsEndpoints(app);
        return app;
    }
}
