using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure;

/// <summary>
/// Sends emails via MailKit/SMTP. Reads config from Email__ settings.
/// Dev: connects to Mailpit (no TLS). Prod: connects to real SMTP with STARTTLS.
/// </summary>
public class MailKitEmailService(
    IConfiguration configuration,
    ILogger<MailKitEmailService> logger) : IEmailService
{
    private string SmtpHost => configuration["Email:SmtpHost"] ?? "localhost";
    private int SmtpPort => int.TryParse(configuration["Email:SmtpPort"], out var p) ? p : 1025;
    private string? SmtpUsername => configuration["Email:SmtpUsername"];
    private string? SmtpPassword => configuration["Email:SmtpPassword"];
    private bool UseSsl => bool.TryParse(configuration["Email:UseSsl"], out var s) && s;
    private string FromAddress => configuration["Email:FromAddress"] ?? "noreply@swiftapp.ch";
    private string FromName => configuration["Email:FromName"] ?? "SwiftApp ERP";

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mime = BuildMimeMessage(message);

        using var client = new SmtpClient();
        try
        {
            var secureOption = UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(SmtpHost, SmtpPort, secureOption, ct);

            if (!string.IsNullOrEmpty(SmtpUsername))
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword, ct);

            await client.SendAsync(mime, ct);
            logger.LogInformation("Email sent to {To}: {Subject}", message.ToAddress, message.Subject);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, ct);
        }
    }

    public async Task SendBulkAsync(IReadOnlyList<EmailMessage> messages, CancellationToken ct = default)
    {
        if (messages.Count == 0) return;

        using var client = new SmtpClient();
        try
        {
            var secureOption = UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(SmtpHost, SmtpPort, secureOption, ct);

            if (!string.IsNullOrEmpty(SmtpUsername))
                await client.AuthenticateAsync(SmtpUsername, SmtpPassword, ct);

            var sent = 0;
            foreach (var message in messages)
            {
                ct.ThrowIfCancellationRequested();
                var mime = BuildMimeMessage(message);
                await client.SendAsync(mime, ct);
                sent++;

                // Throttle: 10 emails per second to avoid SMTP rate limits
                if (sent % 10 == 0)
                    await Task.Delay(1000, ct);
            }

            logger.LogInformation("Bulk email sent: {Count} messages", messages.Count);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, ct);
        }
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(FromName, FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName ?? message.ToAddress, message.ToAddress));
        mime.Subject = message.Subject;

        if (message.ReplyToAddress is not null)
            mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyToAddress));

        mime.Body = new TextPart("html") { Text = message.HtmlBody };
        return mime;
    }
}
