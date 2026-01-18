using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Supplier?> GetBySupplierNumberAsync(string supplierNumber, CancellationToken ct = default);
    Task<PagedResult<Supplier>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task AddAsync(Supplier supplier, CancellationToken ct = default);
    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default);
}
