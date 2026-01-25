using SwiftApp.ERP.Modules.MasterData.Domain.Entities;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

public interface IBillOfMaterialRepository
{
    Task<BillOfMaterial?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BillOfMaterial>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(BillOfMaterial bom, CancellationToken ct = default);
    Task UpdateAsync(BillOfMaterial bom, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
