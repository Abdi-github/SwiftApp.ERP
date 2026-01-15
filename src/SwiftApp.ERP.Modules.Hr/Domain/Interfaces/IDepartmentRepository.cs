using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Domain.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Department?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<PagedResult<Department>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<Department>> GetAllActiveAsync(CancellationToken ct = default);
    Task AddAsync(Department department, CancellationToken ct = default);
    Task UpdateAsync(Department department, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
