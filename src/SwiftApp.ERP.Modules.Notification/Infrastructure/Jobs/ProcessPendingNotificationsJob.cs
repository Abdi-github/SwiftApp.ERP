using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure.Jobs;

/// <summary>
/// Quartz job that processes pending email notifications.
/// Picks up notifications with Status=Pending and Channel=Email|Both,
/// attempts to send them, and updates their status.
/// Runs every 30 seconds.
/// </summary>
[DisallowConcurrentExecution]
public class ProcessPendingNotificationsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ProcessPendingNotificationsJob> logger) : IJob
{
    public const string JobKey = "ProcessPendingNotifications";
    private const int BatchSize = 50;

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var pendingNotifications = await db.Set<Domain.Entities.Notification>()
            .Where(n => n.Status == NotificationStatus.Pending
                        && (n.Channel == NotificationChannel.Email || n.Channel == NotificationChannel.Both)
                        && n.RecipientEmail != null
                        && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(context.CancellationToken);

        // logger.LogDebug("Pending notification batch fetched: count={Count}, batchSize={BatchSize}", pendingNotifications.Count, BatchSize);
        if (pendingNotifications.Count == 0) return;

        logger.LogInformation("Processing {Count} pending email notifications", pendingNotifications.Count);

        foreach (var notification in pendingNotifications)
        {
            try
            {
                var emailMessage = new EmailMessage(
                    notification.RecipientEmail!,
                    null,
                    notification.Subject ?? "(No subject)",
                    notification.Body ?? "");

                await emailService.SendAsync(emailMessage, context.CancellationToken);

                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                notification.RetryCount++;
                notification.ErrorMessage = ex.Message;
                // System.Diagnostics.Debug.WriteLine($"Send failed for notification {notification.Id}, newRetryCount={notification.RetryCount}");
                if (notification.RetryCount >= 3)
                    notification.Status = NotificationStatus.Failed;

                logger.LogWarning(ex,
                    "Failed to send notification {Id}, attempt {Attempt}",
                    notification.Id, notification.RetryCount);
            }
        }

        // TODO: Add dead-letter handling for notifications that repeatedly fail after retries.
        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Processed {Count} pending notifications", pendingNotifications.Count);
    }
}
