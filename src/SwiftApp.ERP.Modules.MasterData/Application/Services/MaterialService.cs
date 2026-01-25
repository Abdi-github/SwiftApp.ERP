using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.MasterData.Application.Services;

public class MaterialService(
    IMaterialRepository materialRepository,
    ILogger<MaterialService> logger)
{
    public async Task<MaterialResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var material = await materialRepository.GetByIdAsync(id, ct);
        return material is null ? null : MapToResponse(material);
    }

    public async Task<MaterialResponse?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var material = await materialRepository.GetBySkuAsync(sku, ct);
        return material is null ? null : MapToResponse(material);
    }

    public async Task<PagedResult<MaterialResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await materialRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<MaterialResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<MaterialResponse> CreateAsync(MaterialRequest request, CancellationToken ct = default)
    {
        if (await materialRepository.GetBySkuAsync(request.Sku, ct) is not null)
            throw new BusinessRuleException("UNIQUE_SKU", $"Material SKU '{request.Sku}' already exists.");

        var material = new Material
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitOfMeasureId = request.UnitOfMeasureId,
            UnitPrice = request.UnitPrice,
            VatRate = request.VatRate,
            MinimumStock = request.MinimumStock,
            Active = request.Active ?? true,
        };

        ApplyTranslations(material, request);
        await materialRepository.AddAsync(material, ct);

        logger.LogInformation("Material {Sku} created with id {MaterialId}", material.Sku, material.Id);
        return MapToResponse(material);
    }

    public async Task<MaterialResponse> UpdateAsync(Guid id, MaterialRequest request, CancellationToken ct = default)
    {
        var material = await materialRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Material), id);

        if (material.Sku != request.Sku && await materialRepository.GetBySkuAsync(request.Sku, ct) is not null)
            throw new BusinessRuleException("UNIQUE_SKU", $"Material SKU '{request.Sku}' already exists.");

        material.Sku = request.Sku;
        material.Name = request.Name;
        material.Description = request.Description;
        material.CategoryId = request.CategoryId;
        material.UnitOfMeasureId = request.UnitOfMeasureId;
        material.UnitPrice = request.UnitPrice;
        material.VatRate = request.VatRate;
        material.MinimumStock = request.MinimumStock;
        material.Active = request.Active ?? material.Active;

        ApplyTranslations(material, request);
        await materialRepository.UpdateAsync(material, ct);

        logger.LogInformation("Material {Sku} updated", material.Sku);
        return MapToResponse(material);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var material = await materialRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Material), id);

        await materialRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Material {Sku} soft-deleted", material.Sku);
    }

    public async Task<bool> IsActiveAsync(Guid id, CancellationToken ct = default)
    {
        var material = await materialRepository.GetByIdAsync(id, ct);
        return material is { Active: true };
    }

    private static void ApplyTranslations(Material material, MaterialRequest request)
    {
        material.Translations.Clear();

        if (request.NameTranslations is null && request.DescriptionTranslations is null)
            return;

        var locales = new HashSet<string>();
        if (request.NameTranslations is not null)
            foreach (var locale in request.NameTranslations.Keys)
                locales.Add(locale);
        if (request.DescriptionTranslations is not null)
            foreach (var locale in request.DescriptionTranslations.Keys)
                locales.Add(locale);

        foreach (var locale in locales)
        {
            material.Translations.Add(new MaterialTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? material.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }
    }

    private static MaterialResponse MapToResponse(Material material) => new(
        material.Id,
        material.Sku,
        material.Name,
        material.Description,
        material.CategoryId,
        material.Category?.Name,
        material.UnitOfMeasureId,
        material.UnitOfMeasure?.Code,
        material.UnitPrice,
        material.VatRate.ToString(),
        material.MinimumStock,
        material.Active,
        material.Translations.Count > 0
            ? material.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        material.Translations.Count > 0
            ? material.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null,
        material.CreatedAt,
        material.UpdatedAt);
}
