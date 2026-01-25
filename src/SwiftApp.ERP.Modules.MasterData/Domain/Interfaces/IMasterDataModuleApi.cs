using SwiftApp.ERP.Modules.MasterData.Application.DTOs;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

public interface IMasterDataModuleApi
{
    Task<ProductResponse?> FindProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductResponse?> FindProductBySkuAsync(string sku, CancellationToken ct = default);
    Task<bool> IsProductActiveAsync(Guid id, CancellationToken ct = default);
    Task<MaterialResponse?> FindMaterialByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsMaterialActiveAsync(Guid id, CancellationToken ct = default);
}
