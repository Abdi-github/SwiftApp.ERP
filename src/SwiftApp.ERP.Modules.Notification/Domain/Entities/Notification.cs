using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Notification.Domain.Entities;

/// <summary>
/// A notification sent to a user (email and/or in-app).
/// Maps to Java: Notification.
/// </summary>
public class Notification : BaseEntity
{
    public Guid RecipientUserId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? TemplateCode { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public string? Subject { get; set; }
    public string? Body { get; set; }

    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }

    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
