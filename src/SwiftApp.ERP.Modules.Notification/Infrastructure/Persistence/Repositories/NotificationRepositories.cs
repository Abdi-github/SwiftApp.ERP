using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure.Persistence.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task<Domain.Entities.Notification?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>().FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<PagedResult<Domain.Entities.Notification>> GetPagedByUserAsync(Guid userId, int page, int size, bool? unreadOnly = null, CancellationToken ct = default)
    {
        var query = db.Set<Domain.Entities.Notification>()
            .Where(n => n.RecipientUserId == userId);

        if (unreadOnly == true)
            query = query.Where(n => n.ReadAt == null && n.Status != NotificationStatus.Dismissed);

        query = query.OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return new PagedResult<Domain.Entities.Notification>(items, page, size, total, (int)Math.Ceiling(total / (double)size));
    }

    public async Task<List<Domain.Entities.Notification>> GetRecentByUserAsync(Guid userId, int count, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>()
            .Where(n => n.RecipientUserId == userId && n.Status != NotificationStatus.Dismissed)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

    public async Task<long> CountUnreadAsync(Guid userId, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>()
            .CountAsync(n => n.RecipientUserId == userId && n.ReadAt == null && n.Status != NotificationStatus.Dismissed, ct);

    public async Task MarkReadAsync(Guid id, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>()
            .Where(n => n.Id == id && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow), ct);

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>()
            .Where(n => n.RecipientUserId == userId && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow), ct);

    public async Task DismissAsync(Guid id, CancellationToken ct) =>
        await db.Set<Domain.Entities.Notification>()
            .Where(n => n.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Status, NotificationStatus.Dismissed), ct);

    public async Task AddAsync(Domain.Entities.Notification notification, CancellationToken ct)
    {
        await db.Set<Domain.Entities.Notification>().AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Domain.Entities.Notification notification, CancellationToken ct)
    {
        db.Set<Domain.Entities.Notification>().Update(notification);
        await db.SaveChangesAsync(ct);
    }
}

public class NotificationTemplateRepository(AppDbContext db) : INotificationTemplateRepository
{
    public async Task<NotificationTemplate?> FindByCodeChannelLocaleAsync(string code, string channel, string locale, CancellationToken ct) =>
        await db.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Code == code && t.Channel.ToString() == channel && t.Locale == locale && t.Active, ct);

    public async Task<List<NotificationTemplate>> GetAllAsync(CancellationToken ct) =>
        await db.Set<NotificationTemplate>().OrderBy(t => t.Code).ToListAsync(ct);

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct)
    {
        await db.Set<NotificationTemplate>().AddAsync(template, ct);
        await db.SaveChangesAsync(ct);
    }
}

public class MailCampaignRepository(AppDbContext db) : IMailCampaignRepository
{
    public async Task<MailCampaign?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Set<MailCampaign>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedResult<MailCampaign>> GetPagedAsync(int page, int size, CancellationToken ct)
    {
        var query = db.Set<MailCampaign>().OrderByDescending(c => c.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new PagedResult<MailCampaign>(items, page, size, total, (int)Math.Ceiling(total / (double)size));
    }

    public async Task AddAsync(MailCampaign campaign, CancellationToken ct)
    {
        await db.Set<MailCampaign>().AddAsync(campaign, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MailCampaign campaign, CancellationToken ct)
    {
        db.Set<MailCampaign>().Update(campaign);
        await db.SaveChangesAsync(ct);
    }
}
