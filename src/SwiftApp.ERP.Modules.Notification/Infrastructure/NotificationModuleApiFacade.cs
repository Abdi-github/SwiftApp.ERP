using SwiftApp.ERP.Modules.Notification.Application.Services;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure;

/// <summary>
/// Implements the cross-module Notification API facade.
/// </summary>
public class NotificationModuleApiFacade(NotificationService notificationService) : INotificationModuleApi
{
    public async Task<long> CountUnreadAsync(Guid userId, CancellationToken ct) =>
        await notificationService.CountUnreadAsync(userId, ct);

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct) =>
        await notificationService.MarkAllReadAsync(userId, ct);

    public async Task SendAdHocAsync(Guid recipientUserId, string? recipientEmail,
        string? templateCode, string subject, string body,
        string? referenceType, Guid? referenceId, CancellationToken ct) =>
        await notificationService.CreateAsync(recipientUserId, recipientEmail, templateCode, subject, body, referenceType, referenceId, ct);
}
