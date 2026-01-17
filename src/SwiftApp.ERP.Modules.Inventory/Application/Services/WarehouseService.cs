using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Inventory.Application.Services;

public class WarehouseService(
    IWarehouseRepository warehouseRepository,
    ILogger<WarehouseService> logger)
{
    public async Task<WarehouseResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, ct);
        return warehouse is null ? null : MapToResponse(warehouse);
    }

    public async Task<WarehouseResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepository.GetByCodeAsync(code, ct);
        return warehouse is null ? null : MapToResponse(warehouse);
    }

    public async Task<PagedResult<WarehouseResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await warehouseRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<WarehouseResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<WarehouseResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var warehouses = await warehouseRepository.GetAllActiveAsync(ct);
        return warehouses.Select(MapToResponse).ToList();
    }

    public async Task<WarehouseResponse> CreateAsync(WarehouseRequest request, CancellationToken ct = default)
    {
        if (await warehouseRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Warehouse code '{request.Code}' already exists.");
        // logger.LogDebug("Create warehouse request: code={Code}, name={Name}, active={Active}", request.Code, request.Name, request.Active);

        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Active = request.Active ?? true,
        };

        ApplyTranslations(warehouse, request);
        await warehouseRepository.AddAsync(warehouse, ct);

        logger.LogInformation("Warehouse {Code} created with id {WarehouseId}", warehouse.Code, warehouse.Id);

        return MapToResponse(warehouse);
    }

    public async Task<WarehouseResponse> UpdateAsync(Guid id, WarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Warehouse), id);

        if (warehouse.Code != request.Code && await warehouseRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Warehouse code '{request.Code}' already exists.");

        warehouse.Code = request.Code;
        warehouse.Name = request.Name;
        warehouse.Description = request.Description;
        warehouse.Address = request.Address;
        warehouse.Active = request.Active ?? warehouse.Active;

        ApplyTranslations(warehouse, request);
        await warehouseRepository.UpdateAsync(warehouse, ct);

        logger.LogInformation("Warehouse {Code} updated", warehouse.Code);

        return MapToResponse(warehouse);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await warehouseRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Warehouse), id);

        await warehouseRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Warehouse {WarehouseId} soft-deleted", id);
    }

    private static void ApplyTranslations(Warehouse warehouse, WarehouseRequest request)
    {
        warehouse.Translations.Clear();

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
            warehouse.Translations.Add(new WarehouseTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? warehouse.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }

        // System.Diagnostics.Debug.WriteLine($"Prepared {warehouse.Translations.Count} warehouse translations for code={warehouse.Code}");
    }

    private static WarehouseResponse MapToResponse(Warehouse warehouse) => new(
        warehouse.Id,
        warehouse.Code,
        warehouse.Name,
        warehouse.Description,
        warehouse.Address,
        warehouse.Active,
        warehouse.CreatedAt,
        warehouse.UpdatedAt,
        warehouse.Translations.Count > 0
            ? warehouse.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        warehouse.Translations.Count > 0
            ? warehouse.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null);
}
