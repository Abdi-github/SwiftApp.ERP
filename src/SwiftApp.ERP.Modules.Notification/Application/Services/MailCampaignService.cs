using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Application.DTOs;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Notification.Application.Services;

public class MailCampaignService(
    IMailCampaignRepository campaignRepo,
    ILogger<MailCampaignService> logger)
{
    public async Task<PagedResult<MailCampaignResponse>> GetPagedAsync(int page, int size, CancellationToken ct)
    {
        var result = await campaignRepo.GetPagedAsync(page, size, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<MailCampaignResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<MailCampaignResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException("MailCampaign", id);
        return MapToResponse(campaign);
    }

    public async Task<MailCampaignResponse> CreateAsync(MailCampaignRequest request, CancellationToken ct)
    {
        var campaign = new MailCampaign
        {
            Name = request.Name,
            Description = request.Description,
            TemplateCode = request.TemplateCode,
            Locale = request.Locale ?? "de",
            TargetSegment = request.TargetSegment,
            Status = MailCampaignStatus.Draft,
            ScheduledAt = request.ScheduledAt,
            SubjectOverride = request.SubjectOverride
        };

        await campaignRepo.AddAsync(campaign, ct);
        logger.LogInformation("Mail campaign created: {Name}", campaign.Name);
        return MapToResponse(campaign);
    }

    private static MailCampaignResponse MapToResponse(MailCampaign c) => new(
        c.Id, c.Name, c.Description, c.TemplateCode,
        c.Locale, c.TargetSegment, c.Status.ToString(),
        c.TotalRecipients, c.SentCount, c.FailedCount,
        c.ScheduledAt, c.StartedAt, c.CompletedAt,
        c.SubjectOverride, c.CreatedBy, c.CreatedAt, c.UpdatedAt);
}
