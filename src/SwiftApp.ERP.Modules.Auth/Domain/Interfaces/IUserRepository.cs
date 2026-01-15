using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<User>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<List<User>> GetAllEnabledAsync(CancellationToken ct = default);
    Task<List<User>> GetAllByRoleAsync(string roleName, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
