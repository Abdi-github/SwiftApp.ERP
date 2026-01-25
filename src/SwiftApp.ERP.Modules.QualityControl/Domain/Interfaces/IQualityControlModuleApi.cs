namespace SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;

public interface IQualityControlModuleApi
{
    Task<long> GetOpenNcrCountAsync(CancellationToken ct = default);
    Task<decimal> GetPassRateAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
