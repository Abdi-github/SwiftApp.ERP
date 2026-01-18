using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<PagedResult<Account>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<List<Account>> GetByTypeAsync(AccountType accountType, CancellationToken ct = default);
    Task<List<Account>> GetRootAccountsAsync(CancellationToken ct = default);
    Task<List<Account>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
