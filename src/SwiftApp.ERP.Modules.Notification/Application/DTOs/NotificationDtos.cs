using SwiftApp.ERP.Modules.Notification.Domain.Enums;

namespace SwiftApp.ERP.Modules.Notification.Application.DTOs;

public record NotificationResponse(
    Guid Id,
    Guid RecipientUserId,
    string? RecipientEmail,
    string? TemplateCode,
    string Channel,
    string Status,
    string? Subject,
    string? Body,
    string? ReferenceType,
    Guid? ReferenceId,
    int RetryCount,
    DateTimeOffset? SentAt,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);

public record MailCampaignRequest(
    string Name,
    string? Description,
    string TemplateCode,
    string? Locale,
    string? TargetSegment,
    DateTimeOffset? ScheduledAt,
    string? SubjectOverride);

public record MailCampaignResponse(
    Guid Id,
    string Name,
    string? Description,
    string? TemplateCode,
    string Locale,
    string? TargetSegment,
    string Status,
    int TotalRecipients,
    int SentCount,
    int FailedCount,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? SubjectOverride,
    string? CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
