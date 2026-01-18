using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Domain.Interfaces;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<PagedResult<SalesOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<PagedResult<SalesOrder>> GetByCustomerAsync(Guid customerId, int page, int size, CancellationToken ct = default);
    Task<List<SalesOrder>> GetByStatusAsync(SalesOrderStatus status, CancellationToken ct = default);
    Task AddAsync(SalesOrder order, CancellationToken ct = default);
    Task UpdateAsync(SalesOrder order, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default);
    Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken ct = default);
    Task<long> GetOpenOrderCountAsync(CancellationToken ct = default);
}
