using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<PagedResult<PurchaseOrder>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<PagedResult<PurchaseOrder>> GetBySupplierAsync(Guid supplierId, int page, int size, CancellationToken ct = default);
    Task<List<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status, CancellationToken ct = default);
    Task AddAsync(PurchaseOrder order, CancellationToken ct = default);
    Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default);
    Task<decimal> GetMonthlySpendAsync(int year, int month, CancellationToken ct = default);
    Task<long> GetOpenOrderCountAsync(CancellationToken ct = default);
}
