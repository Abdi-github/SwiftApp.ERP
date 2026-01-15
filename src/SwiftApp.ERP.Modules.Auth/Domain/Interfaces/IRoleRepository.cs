using SwiftApp.ERP.Modules.Auth.Domain.Entities;

namespace SwiftApp.ERP.Modules.Auth.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Role>> GetAllAsync(CancellationToken ct = default);
    Task<List<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    Task UpdateAsync(Role role, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
