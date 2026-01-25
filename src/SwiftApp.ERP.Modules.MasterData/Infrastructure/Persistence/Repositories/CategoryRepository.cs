using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"CategoryRepository.GetById include Parent+Translations for categoryId={id}");
        => await db.Set<Category>()
            .Include(c => c.ParentCategory)
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        => await db.Set<Category>()
            .Include(c => c.ParentCategory)
            .Include(c => c.Translations)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<List<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
        => await db.Set<Category>()
            .Include(c => c.Translations)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<List<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
        => await db.Set<Category>()
            .Include(c => c.Translations)
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await db.Set<Category>().AddAsync(category, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        db.Set<Category>().Update(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await db.Set<Category>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Category), id);
        category.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
