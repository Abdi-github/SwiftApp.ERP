namespace SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

/// <summary>
/// Abstraction for sending emails via SMTP.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
    Task SendBulkAsync(IReadOnlyList<EmailMessage> messages, CancellationToken ct = default);
}

/// <summary>
/// Represents a single outbound email.
/// </summary>
public record EmailMessage(
    string ToAddress,
    string? ToName,
    string Subject,
    string HtmlBody,
    string? ReplyToAddress = null);
