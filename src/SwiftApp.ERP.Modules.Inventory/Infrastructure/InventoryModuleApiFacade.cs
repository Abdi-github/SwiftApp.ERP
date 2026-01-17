using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Inventory.Infrastructure;

public class InventoryModuleApiFacade(
    IStockLevelRepository stockLevelRepository,
    AppDbContext db) : IInventoryModuleApi
{
    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, CancellationToken ct = default)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, ct);
        return level?.QuantityAvailable ?? 0m;
    }

    public async Task<decimal> GetTotalStockLevelAsync(Guid itemId, CancellationToken ct = default)
    {
        var levels = await stockLevelRepository.GetByItemAsync(itemId, StockItemType.Product, ct);
        return levels.Sum(l => l.QuantityAvailable);
    }

    public async Task<bool> IsStockAvailableAsync(Guid itemId, Guid warehouseId, decimal quantity, CancellationToken ct = default)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, ct);
        return level is not null && level.QuantityAvailable >= quantity;
    }

    public async Task<long> CountLowStockItemsAsync(decimal threshold, CancellationToken ct = default)
    {
        return await db.Set<Domain.Entities.StockLevel>()
            .Where(s => (s.QuantityOnHand - s.QuantityReserved) < threshold)
            .LongCountAsync(ct);
    }
}
