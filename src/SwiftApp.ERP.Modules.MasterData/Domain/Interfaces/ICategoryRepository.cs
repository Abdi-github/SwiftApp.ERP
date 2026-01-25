using SwiftApp.ERP.Modules.MasterData.Domain.Entities;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<List<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<List<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
