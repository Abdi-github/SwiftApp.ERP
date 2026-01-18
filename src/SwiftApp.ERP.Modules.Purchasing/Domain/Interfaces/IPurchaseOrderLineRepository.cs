using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;

public interface IPurchaseOrderLineRepository
{
    Task<List<PurchaseOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(PurchaseOrderLine line, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PurchaseOrderLine> lines, CancellationToken ct = default);
    Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
