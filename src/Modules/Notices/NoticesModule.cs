using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Notices.PayNotice;
using PTickets.Modules.Notices.CancelNotice;
using PTickets.Modules.Notices.GetNotice;
using PTickets.Modules.Notices.GetNoticesForCarForGivenDate;
using PTickets.Modules.Notices.Contracts;

namespace PTickets.Modules.Notices;

public static class NoticesModule
{
    public static IServiceCollection AddNoticesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NoticesDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("NoticesModule")));
      
        services.AddScoped<INoticesModule, NoticeFacade>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NoticesModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapNoticesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPayNoticeEndpoint();
        app.MapCancelNoticeEndpoint();
        app.MapGetNoticeEndpoint();
        app.MapGetNoticesForCarForGivenDateEndpoint();
        return app;
    }
}

