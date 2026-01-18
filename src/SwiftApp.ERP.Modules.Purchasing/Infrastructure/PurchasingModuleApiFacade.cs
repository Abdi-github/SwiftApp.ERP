using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Purchasing.Infrastructure;

public class PurchasingModuleApiFacade(IPurchaseOrderRepository orderRepository) : IPurchasingModuleApi
{
    public async Task<long> GetOpenPurchaseOrderCountAsync(CancellationToken ct = default)
        => await orderRepository.GetOpenOrderCountAsync(ct);

    public async Task<decimal> GetMonthlySpendAsync(int year, int month, CancellationToken ct = default)
        => await orderRepository.GetMonthlySpendAsync(year, month, ct);

    public async Task<decimal> GetOrderTotalAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        return order?.TotalAmount ?? 0m;
    }
}
