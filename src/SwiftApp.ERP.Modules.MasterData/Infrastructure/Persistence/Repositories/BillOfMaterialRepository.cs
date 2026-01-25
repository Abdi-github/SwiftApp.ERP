using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure.Persistence.Repositories;

public class BillOfMaterialRepository(AppDbContext db) : IBillOfMaterialRepository
{
    public async Task<BillOfMaterial?> GetByIdAsync(Guid id, CancellationToken ct = default)
        // System.Diagnostics.Debug.WriteLine($"BillOfMaterialRepository.GetById include material/uom for bomId={id}");
        => await db.Set<BillOfMaterial>()
            .Include(b => b.Material)
            .Include(b => b.UnitOfMeasure)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<List<BillOfMaterial>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
        => await db.Set<BillOfMaterial>()
            .Include(b => b.Material)
            .Include(b => b.UnitOfMeasure)
            .Where(b => b.ProductId == productId)
            .OrderBy(b => b.Position)
            .ToListAsync(ct);

    public async Task AddAsync(BillOfMaterial bom, CancellationToken ct = default)
    {
        await db.Set<BillOfMaterial>().AddAsync(bom, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BillOfMaterial bom, CancellationToken ct = default)
    {
        db.Set<BillOfMaterial>().Update(bom);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var bom = await db.Set<BillOfMaterial>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(BillOfMaterial), id);
        // System.Diagnostics.Debug.WriteLine($"Deleting BOM row id={id}");
        db.Set<BillOfMaterial>().Remove(bom);
        await db.SaveChangesAsync(ct);
    }
}
