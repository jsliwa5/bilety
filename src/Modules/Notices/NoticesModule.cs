using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.AspNetCore.Routing;
using Polly;
using Polly.Timeout;
using PTickets.Modules.Notices.PayNotice;
using PTickets.Modules.Notices.CancelNotice;
using PTickets.Modules.Notices.GetNotice;
using PTickets.Modules.Notices.GetNoticesForCarForGivenDate;
using PTickets.Modules.Notices.Contracts;
using PTickets.Modules.Notices.ExternalSync;

namespace PTickets.Modules.Notices;

public static class NoticesModule
{
    public static IServiceCollection AddNoticesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NoticesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NoticesModule")));

        services.AddScoped<INoticesModule, NoticeFacade>();
        services.AddScoped<DailyNoticesExportService>();

        var externalBaseUrl = configuration["ExternalNoticesSystem:BaseUrl"] ?? "http://localhost:5095";

        services.AddHttpClient("ExternalNoticesClient", client =>
        {
            client.BaseAddress = new Uri(externalBaseUrl);
        })
        .AddResilienceHandler("notices-export-polly-pipeline", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(60));

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            });

            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30)
            });

            builder.AddTimeout(TimeSpan.FromSeconds(10));
        });

        services.AddHostedService<MidnightNoticesExportBackgroundService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NoticesModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapNoticesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPayNoticeEndpoint();
        app.MapCancelNoticeEndpoint();
        app.MapGetNoticeEndpoint();
        app.MapGetNoticesForCarForGivenDateEndpoint();
        app.MapExternalSyncEndpoints();
        return app;
    }
}