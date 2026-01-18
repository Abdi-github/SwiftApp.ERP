using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

public class PurchaseOrderLineRepository(AppDbContext db) : IPurchaseOrderLineRepository
{
    public async Task<List<PurchaseOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await db.Set<PurchaseOrderLine>()
            .Where(l => l.PurchaseOrderId == orderId)
            .OrderBy(l => l.Position)
            .ToListAsync(ct);

    public async Task AddAsync(PurchaseOrderLine line, CancellationToken ct = default)
    {
        await db.Set<PurchaseOrderLine>().AddAsync(line, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<PurchaseOrderLine> lines, CancellationToken ct = default)
    {
        await db.Set<PurchaseOrderLine>().AddRangeAsync(lines, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var lines = await db.Set<PurchaseOrderLine>()
            .Where(l => l.PurchaseOrderId == orderId)
            .ToListAsync(ct);

        db.Set<PurchaseOrderLine>().RemoveRange(lines);
        await db.SaveChangesAsync(ct);
    }
}
