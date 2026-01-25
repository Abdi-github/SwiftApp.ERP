using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Material?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<PagedResult<Material>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task AddAsync(Material material, CancellationToken ct = default);
    Task UpdateAsync(Material material, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
