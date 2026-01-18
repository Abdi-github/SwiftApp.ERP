using SwiftApp.ERP.Modules.Sales.Domain.Entities;

namespace SwiftApp.ERP.Modules.Sales.Domain.Interfaces;

public interface ISalesOrderLineRepository
{
    Task<List<SalesOrderLine>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(SalesOrderLine line, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<SalesOrderLine> lines, CancellationToken ct = default);
    Task DeleteByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
