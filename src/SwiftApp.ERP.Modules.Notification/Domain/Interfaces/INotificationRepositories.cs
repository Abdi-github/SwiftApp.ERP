using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;
using NotificationEntity = SwiftApp.ERP.Modules.Notification.Domain.Entities.Notification;

namespace SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

public interface INotificationRepository
{
    Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<NotificationEntity>> GetPagedByUserAsync(Guid userId, int page, int size, bool? unreadOnly = null, CancellationToken ct = default);
    Task<List<NotificationEntity>> GetRecentByUserAsync(Guid userId, int count, CancellationToken ct = default);
    Task<long> CountUnreadAsync(Guid userId, CancellationToken ct = default);
    Task MarkReadAsync(Guid id, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task DismissAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(NotificationEntity notification, CancellationToken ct = default);
    Task UpdateAsync(NotificationEntity notification, CancellationToken ct = default);
}

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> FindByCodeChannelLocaleAsync(string code, string channel, string locale, CancellationToken ct = default);
    Task<List<NotificationTemplate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(NotificationTemplate template, CancellationToken ct = default);
}

public interface IMailCampaignRepository
{
    Task<MailCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<MailCampaign>> GetPagedAsync(int page, int size, CancellationToken ct = default);
    Task AddAsync(MailCampaign campaign, CancellationToken ct = default);
    Task UpdateAsync(MailCampaign campaign, CancellationToken ct = default);
}
