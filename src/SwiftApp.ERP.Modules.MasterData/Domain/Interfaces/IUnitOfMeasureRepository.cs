using SwiftApp.ERP.Modules.MasterData.Domain.Entities;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

public interface IUnitOfMeasureRepository
{
    Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<UnitOfMeasure>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UnitOfMeasure uom, CancellationToken ct = default);
    Task UpdateAsync(UnitOfMeasure uom, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
