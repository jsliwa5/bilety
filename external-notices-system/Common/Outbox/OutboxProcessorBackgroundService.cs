namespace ExternalNoticesSystem.Api.Common.Outbox;

using ExternalNoticesSystem.Api.Common.Data;
using ExternalNoticesSystem.Api.Common.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Uruchomiono OutboxProcessorBackgroundService.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Nieoczekiwany błąd w pętli OutboxProcessorBackgroundService.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Zatrzymano OutboxProcessorBackgroundService.");
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExternalNoticesDbContext>();
        var webhookSender = scope.ServiceProvider.GetRequiredService<PTicketsWebhookSender>();

        var pendingMessages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 10)
            .OrderBy(m => m.OccurredAt)
            .Take(50)
            .ToListAsync(ct);

        if (pendingMessages.Count == 0)
            return;

        logger.LogInformation("Przetwarzanie {Count} oczekujących wiadomości Outbox...", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                await webhookSender.SendStatusChangedAsync(
                    message.Id,
                    message.NoticeId,
                    message.Action,
                    message.Reason,
                    message.OccurredAt,
                    ct);

                message.MarkAsProcessed(DateTime.UtcNow);
                logger.LogInformation("Pomyślnie wysłano webhook Outbox dla sprawy {NoticeId} (akcja: {Action}).", message.NoticeId, message.Action);
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                logger.LogError(ex, "Niepowodzenie wysyłki webhooka Outbox dla sprawy {NoticeId} (akcja: {Action}, próba {RetryCount}).",
                    message.NoticeId, message.Action, message.RetryCount);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}

