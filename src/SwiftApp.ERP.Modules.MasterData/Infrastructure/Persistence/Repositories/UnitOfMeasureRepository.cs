using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

public class UnitOfMeasureRepository(AppDbContext db) : IUnitOfMeasureRepository
{
    public async Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"UnitOfMeasureRepository.GetById include translations for uomId={id}");
        => await db.Set<UnitOfMeasure>()
            .Include(u => u.Translations)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.Set<UnitOfMeasure>()
            .Include(u => u.Translations)
            .FirstOrDefaultAsync(u => u.Code == code, ct);

    public async Task<List<UnitOfMeasure>> GetAllAsync(CancellationToken ct = default)
        => await db.Set<UnitOfMeasure>()
            .Include(u => u.Translations)
            .OrderBy(u => u.Code)
            .ToListAsync(ct);

    public async Task AddAsync(UnitOfMeasure uom, CancellationToken ct = default)
    {
        await db.Set<UnitOfMeasure>().AddAsync(uom, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UnitOfMeasure uom, CancellationToken ct = default)
    {
        db.Set<UnitOfMeasure>().Update(uom);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var uom = await db.Set<UnitOfMeasure>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(UnitOfMeasure), id);
        uom.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
