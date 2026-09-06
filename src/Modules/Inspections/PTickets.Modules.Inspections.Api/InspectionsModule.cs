using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Inspections.Api.Endpoints;
using PTickets.Modules.Inspections.Application.Commands.StartSession;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Inspections.Infrastructure.Persistence;

namespace PTickets.Modules.Inspections;

public static class InspectionsModule
{
    public static IServiceCollection AddInspectionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InspectionsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("InspectionsDb")));

        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StartSessionCommand).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapInspectionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapSessionEndpoints();
        endpoints.MapInspectionEndpoints();
        endpoints.MapNoticeEndpoints();
        
        return endpoints;
    }
}

