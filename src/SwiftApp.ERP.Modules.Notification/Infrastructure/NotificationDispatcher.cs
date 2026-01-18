using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure;

/// <summary>
/// Central notification dispatcher. Creates DB records for all notifications
/// and immediately sends email for Email/Both channels via IEmailService.
/// InApp notifications are persisted for retrieval via API/SignalR.
/// </summary>
public class NotificationDispatcher(
    INotificationRepository notificationRepo,
    IEmailService emailService,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    private const int MaxRetries = 3;

    public async Task DispatchAsync(NotificationRequest request, CancellationToken ct = default)
    {
        // logger.LogDebug("Dispatch request received: Channel={Channel}, RecipientUserId={RecipientUserId}, Template={TemplateCode}", request.Channel, request.RecipientUserId, request.TemplateCode);
        var notification = new Domain.Entities.Notification
        {
            RecipientUserId = request.RecipientUserId,
            RecipientEmail = request.RecipientEmail,
            Channel = request.Channel,
            TemplateCode = request.TemplateCode,
            Subject = request.Subject,
            Body = request.Body,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            Status = NotificationStatus.Pending
        };

        await notificationRepo.AddAsync(notification, ct);

        // If channel includes Email, attempt sending now
        if (request.Channel is NotificationChannel.Email or NotificationChannel.Both
            && !string.IsNullOrEmpty(request.RecipientEmail))
        {
            await TrySendEmailAsync(notification, ct);
        }
        else if (request.Channel is NotificationChannel.InApp)
        {
            // InApp-only: mark as "sent" (delivered to inbox)
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTimeOffset.UtcNow;
            await notificationRepo.UpdateAsync(notification, ct);
        }

        logger.LogInformation(
            "Notification dispatched: Channel={Channel}, Recipient={UserId}, Subject={Subject}",
            request.Channel, request.RecipientUserId, request.Subject);
    }

    private async Task TrySendEmailAsync(Domain.Entities.Notification notification, CancellationToken ct)
    {
        try
        {
            // System.Diagnostics.Debug.WriteLine($"TrySendEmailAsync for notification={notification.Id}, retryCount={notification.RetryCount}");
            var emailMessage = new EmailMessage(
                notification.RecipientEmail!,
                null,
                notification.Subject ?? "(No subject)",
                notification.Body ?? "");

            await emailService.SendAsync(emailMessage, ct);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTimeOffset.UtcNow;
            await notificationRepo.UpdateAsync(notification, ct);
        }
        catch (Exception ex)
        {
            notification.RetryCount++;
            notification.ErrorMessage = ex.Message;
            notification.Status = notification.RetryCount >= MaxRetries
                ? NotificationStatus.Failed
                : NotificationStatus.Pending;

            await notificationRepo.UpdateAsync(notification, ct);
            logger.LogWarning(ex,
                "Email send failed for notification {Id}, attempt {Attempt}/{Max}",
                notification.Id, notification.RetryCount, MaxRetries);
        }
    }
}
