namespace SwiftApp.ERP.Modules.Production.Domain.Interfaces;

public interface IProductionModuleApi
{
    Task<long> GetActiveProductionOrderCountAsync(CancellationToken ct = default);
    Task<decimal> GetPlannedQuantityByProductAsync(Guid productId, CancellationToken ct = default);
}
