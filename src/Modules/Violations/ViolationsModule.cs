namespace PTickets.Modules.Violations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Violations.Contracts;
using PTickets.Modules.Violations.CreateViolationType;
using PTickets.Modules.Violations.GetAllViolationTypes;
using PTickets.Modules.Violations.SetPenaltyAmount;
using PTickets.Modules.Violations.CreateSurchargeTier;
using PTickets.Modules.Violations.GetAllSurchargeTiers;

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
        services.AddScoped<IViolationsModule, ViolationsModuleFacade>();

        return services;
    }

    public static IEndpointRouteBuilder MapViolationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");
        CreateViolationTypeEndpoint.Map(group);
        GetAllViolationTypesEndpoint.Map(group);
        SetPenaltyAmountEndpoint.Map(group);
        CreateSurchargeTierEndpoint.Map(group);
        GetAllSurchargeTiersEndpoint.Map(group);
        
        return app;
    }
}
