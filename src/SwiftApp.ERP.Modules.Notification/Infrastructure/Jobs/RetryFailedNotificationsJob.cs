using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure.Jobs;

/// <summary>
/// Quartz job that retries failed notifications with exponential backoff.
/// Picks up notifications with Status=Failed or Status=Pending with RetryCount > 0,
/// waits appropriate backoff period, then retries.
/// Runs every 5 minutes.
/// </summary>
[DisallowConcurrentExecution]
public class RetryFailedNotificationsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<RetryFailedNotificationsJob> logger) : IJob
{
    public const string JobKey = "RetryFailedNotifications";
    private const int MaxRetries = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var now = DateTimeOffset.UtcNow;

        // Find failed notifications eligible for retry (with exponential backoff)
        var retryable = await db.Set<Domain.Entities.Notification>()
            .Where(n => n.Status == NotificationStatus.Failed
                        && n.RetryCount < MaxRetries
                        && n.RecipientEmail != null
                        && (n.Channel == NotificationChannel.Email || n.Channel == NotificationChannel.Both))
            .OrderBy(n => n.RetryCount)
            .ThenBy(n => n.CreatedAt)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        // logger.LogDebug("Retry job fetched {Count} notifications eligible by status/channel", retryable.Count);
        if (retryable.Count == 0) return;

        logger.LogInformation("Retrying {Count} failed notifications", retryable.Count);

        foreach (var notification in retryable)
        {
            // Exponential backoff: 1min, 4min, 16min, 64min, 256min
            var backoffMinutes = Math.Pow(4, notification.RetryCount);
            var updatedAt = notification.UpdatedAt ?? notification.CreatedAt;
            if (updatedAt.AddMinutes(backoffMinutes) > now)
                continue; // Not yet time to retry

            try
            {
                var emailMessage = new EmailMessage(
                    notification.RecipientEmail!,
                    null,
                    notification.Subject ?? "(No subject)",
                    notification.Body ?? "");

                await emailService.SendAsync(emailMessage, context.CancellationToken);

                notification.Status = NotificationStatus.Sent;
                notification.SentAt = now;
                notification.ErrorMessage = null;
                logger.LogInformation("Retry succeeded for notification {Id}", notification.Id);
            }
            catch (Exception ex)
            {
                notification.RetryCount++;
                notification.ErrorMessage = ex.Message;
                // System.Diagnostics.Debug.WriteLine($"Retry attempt failed for {notification.Id}, nextRetryCount={notification.RetryCount}");
                if (notification.RetryCount >= MaxRetries)
                {
                    notification.Status = NotificationStatus.Failed;
                    logger.LogError("Notification {Id} permanently failed after {MaxRetries} retries",
                        notification.Id, MaxRetries);
                }
            }
        }

        // TODO: Consider jitter in backoff to avoid synchronized retry spikes.
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
