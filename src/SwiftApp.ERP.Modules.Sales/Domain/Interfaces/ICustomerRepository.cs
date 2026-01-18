using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct = default);
    Task<PagedResult<Customer>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetMaxSequenceForYearAsync(int year, CancellationToken ct = default);
}
