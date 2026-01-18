namespace SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

/// <summary>
/// Cross-module API for the Notification module.
/// Maps to Java: NotificationModuleApi.
/// </summary>
public interface INotificationModuleApi
{
    Task<long> CountUnreadAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task SendAdHocAsync(Guid recipientUserId, string? recipientEmail,
        string? templateCode, string subject, string body,
        string? referenceType, Guid? referenceId, CancellationToken ct = default);
}
