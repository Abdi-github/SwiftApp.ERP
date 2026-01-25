using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Events;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.MasterData.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    IPublisher publisher,
    ILogger<ProductService> logger)
{
    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct);
        return product is null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var product = await productRepository.GetBySkuAsync(sku, ct);
        return product is null ? null : MapToResponse(product);
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await productRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ProductResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct = default)
    {
        if (await productRepository.GetBySkuAsync(request.Sku, ct) is not null)
            throw new BusinessRuleException("UNIQUE_SKU", $"Product SKU '{request.Sku}' already exists.");

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitPrice = request.UnitPrice,
            ListPrice = request.ListPrice,
            VatRate = request.VatRate,
            Active = request.Active ?? true,
        };

        // logger.LogDebug("Creating product SKU={Sku}, CategoryId={CategoryId}, Active={Active}", request.Sku, request.CategoryId, request.Active);
        ApplyTranslations(product, request);
        await productRepository.AddAsync(product, ct);

        logger.LogInformation("Product {Sku} created with id {ProductId}", product.Sku, product.Id);
        await publisher.Publish(new ProductCreatedEvent(product.Id, product.Sku, product.Name), ct);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Product), id);

        if (product.Sku != request.Sku && await productRepository.GetBySkuAsync(request.Sku, ct) is not null)
            throw new BusinessRuleException("UNIQUE_SKU", $"Product SKU '{request.Sku}' already exists.");

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.UnitPrice = request.UnitPrice;
        product.ListPrice = request.ListPrice;
        product.VatRate = request.VatRate;
        product.Active = request.Active ?? product.Active;

        ApplyTranslations(product, request);
        await productRepository.UpdateAsync(product, ct);

        logger.LogInformation("Product {Sku} updated", product.Sku);
        await publisher.Publish(new ProductUpdatedEvent(product.Id, product.Sku), ct);

        return MapToResponse(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Product), id);

        await productRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Product {Sku} soft-deleted", product.Sku);
        await publisher.Publish(new ProductDeletedEvent(product.Id, product.Sku), ct);
    }

    public async Task<bool> IsActiveAsync(Guid id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct);
        return product is { Active: true };
    }

    private static void ApplyTranslations(Product product, ProductRequest request)
    {
        product.Translations.Clear();

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
            product.Translations.Add(new ProductTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? product.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }

        // System.Diagnostics.Debug.WriteLine($"Prepared {product.Translations.Count} product translations for sku={product.Sku}");
    }

    private static ProductResponse MapToResponse(Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.CategoryId,
        product.Category?.Name,
        product.UnitPrice,
        product.ListPrice,
        product.VatRate.ToString(),
        product.Active,
        product.Translations.Count > 0
            ? product.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        product.Translations.Count > 0
            ? product.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null,
        product.CreatedAt,
        product.UpdatedAt);
}
