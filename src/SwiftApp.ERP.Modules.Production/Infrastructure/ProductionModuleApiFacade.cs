using SwiftApp.ERP.Modules.Production.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Production.Infrastructure;

public class ProductionModuleApiFacade(IProductionOrderRepository orderRepository) : IProductionModuleApi
{
    public async Task<long> GetActiveProductionOrderCountAsync(CancellationToken ct = default)
        => await orderRepository.GetActiveOrderCountAsync(ct);

    public async Task<decimal> GetPlannedQuantityByProductAsync(Guid productId, CancellationToken ct = default)
        => await orderRepository.GetPlannedQuantityByProductAsync(productId, ct);
}
