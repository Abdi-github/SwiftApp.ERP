using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Enums;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository(AppDbContext db) : IPurchaseOrderRepository
{
    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<PurchaseOrder>()
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await db.Set<PurchaseOrder>()
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<PagedResult<PurchaseOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<PurchaseOrder>()
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                EF.Functions.ILike(o.OrderNumber, $"%{term}%") ||
                (o.Supplier != null && o.Supplier.CompanyName != null && EF.Functions.ILike(o.Supplier.CompanyName, $"%{term}%")) ||
                (o.Supplier != null && o.Supplier.LastName != null && EF.Functions.ILike(o.Supplier.LastName, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<PurchaseOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<PagedResult<PurchaseOrder>> GetBySupplierAsync(Guid supplierId, int page, int size, CancellationToken ct = default)
    {
        var query = db.Set<PurchaseOrder>()
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .Where(o => o.SupplierId == supplierId);

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<PurchaseOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken ct = default)
        => await db.Set<PurchaseOrder>()
            .Include(o => o.Supplier)
            .Include(o => o.Lines)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

    public async Task AddAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        await db.Set<PurchaseOrder>().AddAsync(order, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        db.Set<PurchaseOrder>().Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Set<PurchaseOrder>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(PurchaseOrder), id);
        order.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default)
    {
        var prefix = $"PO-{year}-";
        var maxNumber = await db.Set<PurchaseOrder>()
            .IgnoreQueryFilters()
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .Select(o => o.OrderNumber)
            .MaxAsync(cancellationToken: ct);

        if (maxNumber is null)
            return 0;

        var seqPart = maxNumber[prefix.Length..];
        return int.TryParse(seqPart, out var seq) ? seq : 0;
    }

    public async Task<decimal> GetMonthlySpendAsync(int year, int month, CancellationToken ct = default)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await db.Set<PurchaseOrder>()
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .Where(o => o.Status != PurchaseOrderStatus.Draft && o.Status != PurchaseOrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);
    }

    public async Task<long> GetOpenOrderCountAsync(CancellationToken ct = default)
        => await db.Set<PurchaseOrder>()
            .Where(o => o.Status != PurchaseOrderStatus.Completed && o.Status != PurchaseOrderStatus.Cancelled)
            .LongCountAsync(ct);
}
