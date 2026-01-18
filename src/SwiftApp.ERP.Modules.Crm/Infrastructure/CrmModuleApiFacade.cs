using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Crm.Infrastructure;

public class CrmModuleApiFacade(AppDbContext db) : ICrmModuleApi
{
    public async Task<long> GetActiveContactCountAsync(CancellationToken ct = default)
        => await db.Set<Contact>()
            .Where(c => c.Active)
            .LongCountAsync(ct);

    public async Task<long> GetUpcomingInteractionCountAsync(CancellationToken ct = default)
        => await db.Set<Interaction>()
            .Where(i => !i.Completed && i.FollowUpDate != null && i.FollowUpDate >= DateTimeOffset.UtcNow)
            .LongCountAsync(ct);
}
