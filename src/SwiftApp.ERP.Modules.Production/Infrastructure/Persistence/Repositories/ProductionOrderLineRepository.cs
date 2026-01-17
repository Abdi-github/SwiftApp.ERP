using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Repositories;

public class ProductionOrderLineRepository(AppDbContext db) : IProductionOrderLineRepository
{
    public async Task<List<ProductionOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await db.Set<ProductionOrderLine>()
            .Where(l => l.ProductionOrderId == orderId)
            .OrderBy(l => l.Position)
            .ToListAsync(ct);

    public async Task AddAsync(ProductionOrderLine line, CancellationToken ct = default)
    {
        await db.Set<ProductionOrderLine>().AddAsync(line, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ProductionOrderLine> lines, CancellationToken ct = default)
    {
        await db.Set<ProductionOrderLine>().AddRangeAsync(lines, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var lines = await db.Set<ProductionOrderLine>()
            .Where(l => l.ProductionOrderId == orderId)
            .ToListAsync(ct);

        db.Set<ProductionOrderLine>().RemoveRange(lines);
        await db.SaveChangesAsync(ct);
    }
}
