using SwiftApp.ERP.Modules.Notification.Domain.Enums;

namespace SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

/// <summary>
/// Dispatches notifications through the appropriate channel(s).
/// Centralizes the decision of how to deliver: Email, InApp, or Both.
/// </summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(NotificationRequest request, CancellationToken ct = default);
}

/// <summary>
/// A fully-resolved notification ready for dispatch.
/// </summary>
public record NotificationRequest(
    Guid RecipientUserId,
    string? RecipientEmail,
    NotificationChannel Channel,
    string? TemplateCode,
    string Subject,
    string Body,
    string? ReferenceType = null,
    Guid? ReferenceId = null);
