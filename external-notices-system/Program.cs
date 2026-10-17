using ExternalNoticesSystem.Api.Common;
using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Outbox;
using ExternalNoticesSystem.Api.Common.Webhooks;
using ExternalNoticesSystem.Api.Features.AppealCase;
using ExternalNoticesSystem.Api.Features.Chaos;
using ExternalNoticesSystem.Api.Features.GetCases;
using ExternalNoticesSystem.Api.Features.GetOutbox;
using ExternalNoticesSystem.Api.Features.PayCase;
using ExternalNoticesSystem.Api.Features.ReceiveBatch;
using ExternalNoticesSystem.Api.Features.ResolveAppeal;
using ExternalNoticesSystem.Api.Features.SendToDebtCollection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5095");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddDbContext<ExternalNoticesDbContext>(options =>
    options.UseSqlite("Data Source=external_notices.db"));

builder.Services.AddSingleton<ChaosState>();
builder.Services.AddScoped<PTicketsWebhookSender>();
builder.Services.AddHostedService<OutboxProcessorBackgroundService>();

var pticketsBaseUrl = builder.Configuration["PTickets:BaseUrl"] ?? "http://localhost:5090";
var apiKey = builder.Configuration["ApiKey"] ?? "secret-webhook-key-123";

builder.Services.AddHttpClient("PTicketsWebhookClient", client =>
{
    client.BaseAddress = new Uri(pticketsBaseUrl);
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
})
.AddResilienceHandler("webhook-polly-pipeline", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromSeconds(30));

    pipeline.AddRetry(new HttpRetryStrategyOptions
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

    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        FailureRatio = 0.5,
        MinimumThroughput = 3,
        BreakDuration = TimeSpan.FromSeconds(15)
    });

    pipeline.AddTimeout(TimeSpan.FromSeconds(5));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExternalNoticesDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "External Notices System API v1");
    c.DocumentTitle = "Zewnętrzny System Zawiadomień i Windykacji";
});

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapChaosEndpoint();
app.MapReceiveBatchEndpoint();
app.MapGetCasesEndpoint();
app.MapGetOutboxEndpoint();
app.MapPayCaseEndpoint();
app.MapAppealCaseEndpoint();
app.MapResolveAppealEndpoint();
app.MapSendToDebtCollectionEndpoint();

app.Run();
