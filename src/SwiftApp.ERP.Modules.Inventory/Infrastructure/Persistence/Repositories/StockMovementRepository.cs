using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(AppDbContext db) : IStockMovementRepository
{
    public async Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<StockMovement>()
            .Include(m => m.SourceWarehouse)
            .Include(m => m.TargetWarehouse)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<StockMovement?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken ct = default)
        => await db.Set<StockMovement>()
            .Include(m => m.SourceWarehouse)
            .Include(m => m.TargetWarehouse)
            .FirstOrDefaultAsync(m => m.ReferenceNumber == referenceNumber, ct);

    public async Task<PagedResult<StockMovement>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var query = db.Set<StockMovement>()
            .Include(m => m.SourceWarehouse)
            .Include(m => m.TargetWarehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                EF.Functions.ILike(m.ReferenceNumber, $"%{term}%"));
        }

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<StockMovement>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<StockMovement>> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken ct = default)
        => await db.Set<StockMovement>()
            .Include(m => m.SourceWarehouse)
            .Include(m => m.TargetWarehouse)
            .Where(m => m.ItemId == itemId && m.ItemType == itemType)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync(ct);

    public async Task AddAsync(StockMovement movement, CancellationToken ct = default)
    {
        await db.Set<StockMovement>().AddAsync(movement, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> CountTodayByTypeAsync(MovementType type, CancellationToken ct = default)
    {
        var todayUtc = DateTimeOffset.UtcNow.Date;
        return await db.Set<StockMovement>()
            .CountAsync(m => m.MovementType == type && m.MovementDate >= todayUtc, ct);
    }
}
