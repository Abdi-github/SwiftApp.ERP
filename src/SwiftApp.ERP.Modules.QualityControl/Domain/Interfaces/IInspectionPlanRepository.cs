using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;

public interface IInspectionPlanRepository
{
    Task<InspectionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<InspectionPlan?> GetByPlanNumberAsync(string planNumber, CancellationToken ct = default);
    Task<PagedResult<InspectionPlan>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionPlan>> GetActiveAsync(CancellationToken ct = default);
    Task<string> GetNextPlanNumberAsync(CancellationToken ct = default);
    Task AddAsync(InspectionPlan plan, CancellationToken ct = default);
    Task UpdateAsync(InspectionPlan plan, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
