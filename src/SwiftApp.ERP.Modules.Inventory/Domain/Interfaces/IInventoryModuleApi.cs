namespace SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;

public interface IInventoryModuleApi
{
    Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, CancellationToken ct = default);
    Task<decimal> GetTotalStockLevelAsync(Guid itemId, CancellationToken ct = default);
    Task<bool> IsStockAvailableAsync(Guid itemId, Guid warehouseId, decimal quantity, CancellationToken ct = default);
    Task<long> CountLowStockItemsAsync(decimal threshold, CancellationToken ct = default);
}
