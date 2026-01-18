using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Notification.Domain.Entities;

/// <summary>
/// Localized notification template (subject + body per code/channel/locale).
/// Maps to Java: NotificationTemplate.
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;
    public string Locale { get; set; } = "de";
    public string? Subject { get; set; }
    public string? BodyTemplate { get; set; }
    public bool Active { get; set; } = true;
}
