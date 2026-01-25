using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.MasterData.Application.Services;

public class BillOfMaterialService(
    IBillOfMaterialRepository bomRepository,
    ILogger<BillOfMaterialService> logger)
{
    public async Task<BillOfMaterialResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var bom = await bomRepository.GetByIdAsync(id, ct);
        return bom is null ? null : MapToResponse(bom);
    }

    public async Task<List<BillOfMaterialResponse>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var boms = await bomRepository.GetByProductIdAsync(productId, ct);
        return boms.Select(MapToResponse).ToList();
    }

    public async Task<BillOfMaterialResponse> CreateAsync(Guid productId, BillOfMaterialRequest request, CancellationToken ct = default)
    {
        var bom = new BillOfMaterial
        {
            ProductId = productId,
            MaterialId = request.MaterialId,
            Quantity = request.Quantity,
            UnitOfMeasureId = request.UnitOfMeasureId,
            Position = request.Position,
            Notes = request.Notes,
        };

        await bomRepository.AddAsync(bom, ct);
        logger.LogInformation("BOM entry created for product {ProductId}, material {MaterialId}", productId, request.MaterialId);

        // Reload with navigations
        var created = await bomRepository.GetByIdAsync(bom.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<BillOfMaterialResponse> UpdateAsync(Guid id, BillOfMaterialRequest request, CancellationToken ct = default)
    {
        var bom = await bomRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(BillOfMaterial), id);

        bom.MaterialId = request.MaterialId;
        bom.Quantity = request.Quantity;
        bom.UnitOfMeasureId = request.UnitOfMeasureId;
        bom.Position = request.Position;
        bom.Notes = request.Notes;

        await bomRepository.UpdateAsync(bom, ct);
        logger.LogInformation("BOM entry {BomId} updated", id);

        var updated = await bomRepository.GetByIdAsync(bom.Id, ct);
        return MapToResponse(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await bomRepository.DeleteAsync(id, ct);
        logger.LogInformation("BOM entry {BomId} deleted", id);
    }

    private static BillOfMaterialResponse MapToResponse(BillOfMaterial bom) => new(
        bom.Id,
        bom.ProductId,
        bom.MaterialId,
        bom.Material?.Sku ?? string.Empty,
        bom.Material?.Name ?? string.Empty,
        bom.Quantity,
        bom.UnitOfMeasureId,
        bom.UnitOfMeasure?.Code,
        bom.Position,
        bom.Notes,
        bom.CreatedAt,
        bom.UpdatedAt);
}
