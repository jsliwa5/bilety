using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Notices.Domain;
using PTickets.Modules.Notices.Infrastructure.Persistence;
using PTickets.Modules.Notices.Endpoints;

namespace PTickets.Modules.Notices;

public static class NoticesModule
{
    public static IServiceCollection AddNoticesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NoticesDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("NoticesModule")));
        
        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NoticesModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapNoticesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapNoticeEndpoints();
        return app;
    }
}
