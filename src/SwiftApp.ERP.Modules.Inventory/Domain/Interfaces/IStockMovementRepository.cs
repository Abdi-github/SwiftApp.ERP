using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;

public interface IStockMovementRepository
{
    Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StockMovement?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken ct = default);
    Task<PagedResult<StockMovement>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovement>> GetByItemAsync(Guid itemId, StockItemType itemType, CancellationToken ct = default);
    Task AddAsync(StockMovement movement, CancellationToken ct = default);
    Task<int> CountTodayByTypeAsync(MovementType type, CancellationToken ct = default);
}
