using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

public class StockLevelRepository(AppDbContext db) : IStockLevelRepository
{
    public async Task<StockLevel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<StockLevel>()
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<StockLevel?> GetByItemAndWarehouseAsync(Guid itemId, StockItemType itemType, Guid warehouseId, CancellationToken ct = default)
        => await db.Set<StockLevel>()
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.ItemId == itemId && s.ItemType == itemType && s.WarehouseId == warehouseId, ct);

    public async Task<IReadOnlyList<StockLevel>> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken ct = default)
        => await db.Set<StockLevel>()
            .Include(s => s.Warehouse)
            .Where(s => s.ItemId == itemId && s.ItemType == itemType)
            .OrderBy(s => s.Warehouse!.Code)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StockLevel>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
        => await db.Set<StockLevel>()
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == warehouseId)
            .OrderBy(s => s.ItemId)
            .ToListAsync(ct);

    public async Task<PagedResult<StockLevel>> GetPagedAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Set<StockLevel>()
            .Include(s => s.Warehouse)
            .AsQueryable();

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderBy(s => s.Warehouse!.Code)
            .ThenBy(s => s.ItemId)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<StockLevel>(items, page, size, totalItems, totalPages);
    }

    public async Task AddAsync(StockLevel stockLevel, CancellationToken ct = default)
    {
        await db.Set<StockLevel>().AddAsync(stockLevel, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(StockLevel stockLevel, CancellationToken ct = default)
    {
        db.Set<StockLevel>().Update(stockLevel);
        await db.SaveChangesAsync(ct);
    }
}
