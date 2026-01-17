using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Interfaces;

public interface IProductionOrderRepository
{
    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductionOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<PagedResult<ProductionOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<List<ProductionOrder>> GetByStatusAsync(ProductionOrderStatus status, CancellationToken ct = default);
    Task<PagedResult<ProductionOrder>> GetByWorkCenterAsync(Guid workCenterId, int page, int size, CancellationToken ct = default);
    Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default);
    Task<long> GetActiveOrderCountAsync(CancellationToken ct = default);
    Task<decimal> GetPlannedQuantityByProductAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(ProductionOrder order, CancellationToken ct = default);
    Task UpdateAsync(ProductionOrder order, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
