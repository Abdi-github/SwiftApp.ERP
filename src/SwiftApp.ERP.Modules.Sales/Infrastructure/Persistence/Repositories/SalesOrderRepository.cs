using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Sales.Infrastructure.Persistence.Repositories;

public class SalesOrderRepository(AppDbContext db) : ISalesOrderRepository
{
    public async Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<SalesOrder>()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await db.Set<SalesOrder>()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<PagedResult<SalesOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<SalesOrder>()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                EF.Functions.ILike(o.OrderNumber, $"%{term}%") ||
                (o.Customer != null && o.Customer.CompanyName != null && EF.Functions.ILike(o.Customer.CompanyName, $"%{term}%")) ||
                (o.Customer != null && o.Customer.LastName != null && EF.Functions.ILike(o.Customer.LastName, $"%{term}%")));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderNumber)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<SalesOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<PagedResult<SalesOrder>> GetByCustomerAsync(Guid customerId, int page, int size, CancellationToken ct = default)
    {
        var query = db.Set<SalesOrder>()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Where(o => o.CustomerId == customerId);

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<SalesOrder>(items, page, size, totalItems, totalPages);
    }

    public async Task<List<SalesOrder>> GetByStatusAsync(SalesOrderStatus status, CancellationToken ct = default)
        => await db.Set<SalesOrder>()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

    public async Task AddAsync(SalesOrder order, CancellationToken ct = default)
    {
        await db.Set<SalesOrder>().AddAsync(order, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SalesOrder order, CancellationToken ct = default)
    {
        db.Set<SalesOrder>().Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Set<SalesOrder>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(SalesOrder), id);
        order.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default)
    {
        var prefix = $"SO-{year}-";
        var maxNumber = await db.Set<SalesOrder>()
            .IgnoreQueryFilters()
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .Select(o => o.OrderNumber)
            .MaxAsync(cancellationToken: ct);

        if (maxNumber is null)
            return 0;

        var seqPart = maxNumber[prefix.Length..];
        return int.TryParse(seqPart, out var seq) ? seq : 0;
    }

    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken ct = default)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await db.Set<SalesOrder>()
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .Where(o => o.Status != SalesOrderStatus.Draft && o.Status != SalesOrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, ct);
    }

    public async Task<long> GetOpenOrderCountAsync(CancellationToken ct = default)
        => await db.Set<SalesOrder>()
            .Where(o => o.Status != SalesOrderStatus.Completed && o.Status != SalesOrderStatus.Cancelled)
            .LongCountAsync(ct);
}
