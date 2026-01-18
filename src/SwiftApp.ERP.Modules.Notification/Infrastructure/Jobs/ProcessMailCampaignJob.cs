using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure.Jobs;

/// <summary>
/// Quartz job that processes queued mail campaigns.
/// Picks up campaigns with Status=Queued (or scheduled campaigns whose time has arrived),
/// resolves recipients from target segment, and sends emails in batches.
/// Runs every minute.
/// </summary>
[DisallowConcurrentExecution]
public class ProcessMailCampaignJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ProcessMailCampaignJob> logger) : IJob
{
    public const string JobKey = "ProcessMailCampaign";
    private const int BatchSize = 25;

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateRenderer = scope.ServiceProvider.GetRequiredService<ITemplateRenderer>();
        var campaignRepo = scope.ServiceProvider.GetRequiredService<IMailCampaignRepository>();
        var now = DateTimeOffset.UtcNow;

        // Find campaigns ready to run
        var campaigns = await db.Set<MailCampaign>()
            .Where(c => c.Status == MailCampaignStatus.Queued
                        || (c.Status == MailCampaignStatus.Draft && c.ScheduledAt != null && c.ScheduledAt <= now))
            .OrderBy(c => c.ScheduledAt ?? c.CreatedAt)
            .Take(5)
            .ToListAsync(context.CancellationToken);


        foreach (var campaign in campaigns)
        {
            await ProcessCampaignAsync(campaign, db, emailService, templateRenderer, campaignRepo, context.CancellationToken);
        }
    }

    private async Task ProcessCampaignAsync(
        MailCampaign campaign,
        AppDbContext db,
        IEmailService emailService,
        ITemplateRenderer templateRenderer,
        IMailCampaignRepository campaignRepo,
        CancellationToken ct)
    {
        logger.LogInformation("Starting mail campaign: {Name} (ID={Id})", campaign.Name, campaign.Id);

        campaign.Status = MailCampaignStatus.Running;
        campaign.StartedAt = DateTimeOffset.UtcNow;
        await campaignRepo.UpdateAsync(campaign, ct);

        try
        {
            // Resolve recipients from target segment
            var recipients = await ResolveRecipientsAsync(db, campaign.TargetSegment, ct);
            campaign.TotalRecipients = recipients.Count;
            // logger.LogDebug("Campaign {CampaignId} recipient resolution: segment={Segment}, count={RecipientCount}", campaign.Id, campaign.TargetSegment, recipients.Count);

            if (recipients.Count == 0)
            {
                campaign.Status = MailCampaignStatus.Completed;
                campaign.CompletedAt = DateTimeOffset.UtcNow;
                await campaignRepo.UpdateAsync(campaign, ct);
                logger.LogWarning("Campaign {Name} has no recipients for segment '{Segment}'",
                    campaign.Name, campaign.TargetSegment);
                return;
            }

            // Render template
            var variables = new Dictionary<string, object?>
            {
                ["campaign_name"] = campaign.Name,
                ["campaign_description"] = campaign.Description
            };

            var rendered = campaign.TemplateCode is not null
                ? await templateRenderer.RenderAsync(campaign.TemplateCode, "Email", campaign.Locale, variables, ct)
                : new Domain.Interfaces.RenderedTemplate(
                    campaign.SubjectOverride ?? campaign.Name,
                    campaign.Description ?? "");

            var subject = campaign.SubjectOverride ?? rendered.Subject;

            // Send in batches
            var batch = new List<EmailMessage>();
            foreach (var (email, name) in recipients)
            {
                batch.Add(new EmailMessage(email, name, subject, rendered.Body));

                if (batch.Count >= BatchSize)
                {
                    await SendBatchAsync(batch, campaign, emailService, campaignRepo, ct);
                    batch.Clear();
                }
            }

            // Send remaining
            if (batch.Count > 0)
                await SendBatchAsync(batch, campaign, emailService, campaignRepo, ct);

            campaign.Status = MailCampaignStatus.Completed;
            campaign.CompletedAt = DateTimeOffset.UtcNow;
            await campaignRepo.UpdateAsync(campaign, ct);

            logger.LogInformation(
                "Campaign {Name} completed: {Sent} sent, {Failed} failed out of {Total}",
                campaign.Name, campaign.SentCount, campaign.FailedCount, campaign.TotalRecipients);
        }
        catch (Exception ex)
        {
            campaign.Status = MailCampaignStatus.Failed;
            campaign.CompletedAt = DateTimeOffset.UtcNow;
            await campaignRepo.UpdateAsync(campaign, ct);
            logger.LogError(ex, "Campaign {Name} failed", campaign.Name);
        }
    }

    private async Task SendBatchAsync(
        List<EmailMessage> batch,
        MailCampaign campaign,
        IEmailService emailService,
        IMailCampaignRepository campaignRepo,
        CancellationToken ct)
    {
        try
        {
            // System.Diagnostics.Debug.WriteLine($"Sending campaign batch for {campaign.Name}: batchSize={batch.Count}");
            await emailService.SendBulkAsync(batch, ct);
            campaign.SentCount += batch.Count;
        }
        catch (Exception ex)
        {
            campaign.FailedCount += batch.Count;
            logger.LogWarning(ex, "Batch send failed for campaign {Name}, {Count} messages",
                campaign.Name, batch.Count);
        }

        await campaignRepo.UpdateAsync(campaign, ct);
    }

    /// <summary>
    /// Resolves email recipients from a target segment.
    /// Segments: "all_employees", "all_customers", "all_contacts", "all_users"
    /// </summary>
    private static async Task<List<(string Email, string? Name)>> ResolveRecipientsAsync(
        AppDbContext db, string? segment, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(segment))
            return [];

        // TODO: Replace raw SQL segment queries with centralized recipient view for consistency and easier filtering.

        return segment.ToLowerInvariant() switch
        {
            "all_employees" => await db.Database
                .SqlQueryRaw<EmailRecipient>(
                    "SELECT email, first_name || ' ' || last_name AS name FROM employees WHERE deleted_at IS NULL AND email IS NOT NULL")
                .Select(r => ValueTuple.Create(r.Email, (string?)r.Name))
                .ToListAsync(ct),

            "all_customers" => await db.Database
                .SqlQueryRaw<EmailRecipient>(
                    "SELECT email, name FROM customers WHERE deleted_at IS NULL AND email IS NOT NULL")
                .Select(r => ValueTuple.Create(r.Email, (string?)r.Name))
                .ToListAsync(ct),

            "all_contacts" => await db.Database
                .SqlQueryRaw<EmailRecipient>(
                    "SELECT email, first_name || ' ' || last_name AS name FROM contacts WHERE deleted_at IS NULL AND email IS NOT NULL")
                .Select(r => ValueTuple.Create(r.Email, (string?)r.Name))
                .ToListAsync(ct),

            "all_users" => await db.Database
                .SqlQueryRaw<EmailRecipient>(
                    "SELECT email, first_name || ' ' || last_name AS name FROM users WHERE deleted_at IS NULL AND email IS NOT NULL AND is_active = true")
                .Select(r => ValueTuple.Create(r.Email, (string?)r.Name))
                .ToListAsync(ct),

            _ => []
        };
    }

    private record EmailRecipient(string Email, string? Name);
}
