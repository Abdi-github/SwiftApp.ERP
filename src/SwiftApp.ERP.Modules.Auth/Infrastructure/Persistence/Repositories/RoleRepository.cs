using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.Repositories;

public class RoleRepository(AppDbContext db) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => await db.Set<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
        => await db.Set<Role>()
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<List<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct = default)
        => await db.Set<Role>()
            .Include(r => r.Permissions)
            .Where(r => names.Contains(r.Name))
            .ToListAsync(ct);

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await db.Set<Role>().AddAsync(role, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        db.Set<Role>().Update(role);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await db.Set<Role>().FindAsync([id], ct)
            ?? throw new SwiftApp.ERP.SharedKernel.Exceptions.EntityNotFoundException(nameof(Role), id);
        role.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
