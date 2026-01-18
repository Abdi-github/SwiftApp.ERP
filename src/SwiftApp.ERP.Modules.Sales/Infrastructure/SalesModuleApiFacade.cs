using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Sales.Infrastructure;

public class SalesModuleApiFacade(ISalesOrderRepository orderRepository) : ISalesModuleApi
{
    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken ct = default)
        => await orderRepository.GetMonthlyRevenueAsync(year, month, ct);

    public async Task<long> GetOpenOrderCountAsync(CancellationToken ct = default)
        => await orderRepository.GetOpenOrderCountAsync(ct);

    public async Task<decimal> GetOrderTotalAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        return order?.TotalAmount ?? 0m;
    }
}
