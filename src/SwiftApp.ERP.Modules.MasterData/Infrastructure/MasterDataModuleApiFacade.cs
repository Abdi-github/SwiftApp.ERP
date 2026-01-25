using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.MasterData.Infrastructure;

public class MasterDataModuleApiFacade(
    ProductService productService,
    MaterialService materialService) : IMasterDataModuleApi
{
    public async Task<ProductResponse?> FindProductByIdAsync(Guid id, CancellationToken ct = default)
        => await productService.GetByIdAsync(id, ct);

    public async Task<ProductResponse?> FindProductBySkuAsync(string sku, CancellationToken ct = default)
        => await productService.GetBySkuAsync(sku, ct);

    public async Task<bool> IsProductActiveAsync(Guid id, CancellationToken ct = default)
        => await productService.IsActiveAsync(id, ct);

    public async Task<MaterialResponse?> FindMaterialByIdAsync(Guid id, CancellationToken ct = default)
        => await materialService.GetByIdAsync(id, ct);

    public async Task<bool> IsMaterialActiveAsync(Guid id, CancellationToken ct = default)
        => await materialService.IsActiveAsync(id, ct);
}
