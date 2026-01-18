namespace SwiftApp.ERP.Modules.Notification.Domain.Enums;

public enum NotificationChannel
{
    Email,
    InApp,
    Both
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Read,
    Dismissed
}

public enum MailCampaignStatus
{
    Draft,
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}
