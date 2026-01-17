using SwiftApp.ERP.Modules.Production.Domain.Entities;

namespace SwiftApp.ERP.Modules.Production.Domain.Interfaces;

public interface IProductionOrderLineRepository
{
    Task<List<ProductionOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(ProductionOrderLine line, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ProductionOrderLine> lines, CancellationToken ct = default);
    Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
