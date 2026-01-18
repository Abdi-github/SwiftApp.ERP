using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Sales.Infrastructure.Persistence.Repositories;

public class SalesOrderLineRepository(AppDbContext db) : ISalesOrderLineRepository
{
    public async Task<List<SalesOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await db.Set<SalesOrderLine>()
            .Where(l => l.SalesOrderId == orderId)
            .OrderBy(l => l.Position)
            .ToListAsync(ct);

    public async Task AddAsync(SalesOrderLine line, CancellationToken ct = default)
    {
        await db.Set<SalesOrderLine>().AddAsync(line, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<SalesOrderLine> lines, CancellationToken ct = default)
    {
        await db.Set<SalesOrderLine>().AddRangeAsync(lines, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var lines = await db.Set<SalesOrderLine>()
            .Where(l => l.SalesOrderId == orderId)
            .ToListAsync(ct);

        db.Set<SalesOrderLine>().RemoveRange(lines);
        await db.SaveChangesAsync(ct);
    }
}
