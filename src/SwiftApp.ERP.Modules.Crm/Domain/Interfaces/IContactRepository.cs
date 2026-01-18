using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Domain.Interfaces;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Contact>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<Contact>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Contact contact, CancellationToken ct = default);
    Task UpdateAsync(Contact contact, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
