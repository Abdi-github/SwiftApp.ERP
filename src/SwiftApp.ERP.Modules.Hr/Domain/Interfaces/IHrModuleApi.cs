namespace SwiftApp.ERP.Modules.Hr.Domain.Interfaces;

public interface IHrModuleApi
{
    Task<long> GetActiveEmployeeCountAsync(CancellationToken ct = default);
    Task<long> GetDepartmentCountAsync(CancellationToken ct = default);
}
