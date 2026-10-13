using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PTickets.Modules.Notices.ExternalSync;

public class MidnightNoticesExportBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MidnightNoticesExportBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("MidnightNoticesExportBackgroundService został uruchomiony.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1); // Najbliższa północ (00:00:00)
            var delay = nextMidnight - now;

            logger.LogInformation("Kolejny automatyczny eksport zawiadomień o północy ({Midnight}, za {Delay}).", nextMidnight, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunExportAsync(stoppingToken);
        }
    }

    private async Task RunExportAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var exportService = scope.ServiceProvider.GetRequiredService<DailyNoticesExportService>();
            var exportedCount = await exportService.ExportUnsyncedNoticesAsync(ct);

            logger.LogInformation("Nocny eksport zakończony. Wysłano łącznie: {Count} zawiadomień.", exportedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Wszystkie próby Polly zawiodły podczas nocnego eksportu zawiadomień. Rekordy pozostają z SyncedAt = null i zostaną wysłane przy kolejnej próbie.");
        }
    }
}