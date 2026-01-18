using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Notification.Domain.Entities;

/// <summary>
/// Bulk email campaign with tracking.
/// Maps to Java: MailCampaign.
/// </summary>
public class MailCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TemplateCode { get; set; }
    public string Locale { get; set; } = "de";
    public string? TargetSegment { get; set; }

    public MailCampaignStatus Status { get; set; } = MailCampaignStatus.Draft;

    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public string? SubjectOverride { get; set; }
}
