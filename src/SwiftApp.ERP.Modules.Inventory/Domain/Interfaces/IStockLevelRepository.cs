using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;

public interface IStockLevelRepository
{
    Task<StockLevel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StockLevel?> GetByItemAndWarehouseAsync(Guid itemId, StockItemType itemType, Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<StockLevel>> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken ct = default);
    Task<IReadOnlyList<StockLevel>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<PagedResult<StockLevel>> GetPagedAsync(int page, int size, CancellationToken ct = default);
    Task AddAsync(StockLevel stockLevel, CancellationToken ct = default);
    Task UpdateAsync(StockLevel stockLevel, CancellationToken ct = default);
}
