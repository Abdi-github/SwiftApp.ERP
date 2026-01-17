using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Repositories;

public class ProductionOrderRepository(AppDbContext db) : IProductionOrderRepository
{
    public async Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<ProductionOrder>()
            .Include(o => o.WorkCenter)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<ProductionOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await db.Set<ProductionOrder>()
            .Include(o => o.WorkCenter)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<PagedResult<ProductionOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<ProductionOrder>()
            .Include(o => o.WorkCenter)
            .Include(o => o.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                EF.Functions.ILike(o.OrderNumber, $"%{term}%") ||
                (o.Notes != null && EF.Functions.ILike(o.Notes, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.PlannedStartDate)
            .ThenByDescending(o => o.OrderNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ProductionOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<ProductionOrder>> GetByStatusAsync(ProductionOrderStatus status, CancellationToken ct = default)
        => await db.Set<ProductionOrder>()
            .Include(o => o.WorkCenter)
            .Include(o => o.Lines)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.PlannedStartDate)
            .ToListAsync(ct);

    public async Task<PagedResult<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId, int page, int size, CancellationToken ct = default)
    {
        var query = db.Set<ProductionOrder>()
            .Include(o => o.WorkCenter)
            .Include(o => o.Lines)
            .Where(o => o.WorkCenterId == workCenterId);

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.PlannedStartDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ProductionOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default)
    {
        var prefix = $"MO-{year}-";
        var maxNumber = await db.Set<ProductionOrder>()
            .IgnoreQueryFilters()
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .Select(o => o.OrderNumber)
            .MaxAsync(cancellationToken: ct);

        if (maxNumber is null)
            return 0;

        var seqPart = maxNumber[prefix.Length..];
        return int.TryParse(seqPart, out var seq) ? seq : 0;
    }

    public async Task<long> GetActiveOrderCountAsync(CancellationToken ct = default)
        => await db.Set<ProductionOrder>()
            .Where(o => o.Status == ProductionOrderStatus.Released || o.Status == ProductionOrderStatus.InProgress)
            .LongCountAsync(ct);

    public async Task<decimal> GetPlannedQuantityByProductAsync(Guid productId, CancellationToken ct = default)
        => await db.Set<ProductionOrder>()
            .Where(o => o.ProductId == productId && o.Status != ProductionOrderStatus.Cancelled && o.Status != ProductionOrderStatus.Completed)
            .SumAsync(o => o.PlannedQuantity, ct);

    public async Task AddAsync(ProductionOrder order, CancellationToken ct = default)
    {
        await db.Set<ProductionOrder>().AddAsync(order, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProductionOrder order, CancellationToken ct = default)
    {
        db.Set<ProductionOrder>().Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Set<ProductionOrder>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(ProductionOrder), id);
        order.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
