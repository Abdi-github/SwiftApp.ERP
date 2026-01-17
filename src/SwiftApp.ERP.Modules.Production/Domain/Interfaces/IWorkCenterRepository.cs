using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Interfaces;

public interface IWorkCenterRepository
{
    Task<WorkCenter?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkCenter?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<PagedResult<WorkCenter>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<WorkCenter>> GetAllActiveAsync(CancellationToken ct = default);
    Task AddAsync(WorkCenter workCenter, CancellationToken ct = default);
    Task UpdateAsync(WorkCenter workCenter, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
