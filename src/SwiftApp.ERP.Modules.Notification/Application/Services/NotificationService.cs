using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Application.DTOs;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Notification.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepo,
    ILogger<NotificationService> logger)
{
    public async Task<PagedResult<NotificationResponse>> GetByUserAsync(Guid userId, int page, int size, bool? unreadOnly, CancellationToken ct)
    {
        var result = await notificationRepo.GetPagedByUserAsync(userId, page, size, unreadOnly, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<NotificationResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<List<NotificationResponse>> GetRecentByUserAsync(Guid userId, int count, CancellationToken ct)
    {
        var items = await notificationRepo.GetRecentByUserAsync(userId, count, ct);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<long> CountUnreadAsync(Guid userId, CancellationToken ct) =>
        await notificationRepo.CountUnreadAsync(userId, ct);

    public async Task MarkReadAsync(Guid notificationId, CancellationToken ct)
    {
        await notificationRepo.MarkReadAsync(notificationId, ct);
        logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct) =>
        await notificationRepo.MarkAllReadAsync(userId, ct);

    public async Task DismissAsync(Guid notificationId, CancellationToken ct)
    {
        await notificationRepo.DismissAsync(notificationId, ct);
        logger.LogInformation("Notification {NotificationId} dismissed", notificationId);
    }

    public async Task CreateAsync(Guid recipientUserId, string? recipientEmail,
        string? templateCode, string subject, string body,
        string? referenceType, Guid? referenceId, CancellationToken ct)
    {
        // logger.LogDebug("Create notification request: principalId={PrincipalId}, hasDirectContact={HasDirectContact}, template={TemplateCode}", recipientUserId, !string.IsNullOrWhiteSpace(recipientEmail), templateCode);
        var notification = new Domain.Entities.Notification
        {
            RecipientUserId = recipientUserId,
            RecipientEmail = recipientEmail,
            TemplateCode = templateCode,
            Channel = NotificationChannel.InApp,
            Status = NotificationStatus.Pending,
            Subject = subject,
            Body = body,
            ReferenceType = referenceType,
            ReferenceId = referenceId
        };

        await notificationRepo.AddAsync(notification, ct);

        logger.LogInformation("Notification created for user {UserId}: {Subject}", recipientUserId, subject);
    }

    private static NotificationResponse MapToResponse(Domain.Entities.Notification n) => new(
        n.Id, n.RecipientUserId, n.RecipientEmail, n.TemplateCode,
        n.Channel.ToString(), n.Status.ToString(),
        n.Subject, n.Body, n.ReferenceType, n.ReferenceId,
        n.RetryCount, n.SentAt, n.ReadAt, n.CreatedAt);
}
