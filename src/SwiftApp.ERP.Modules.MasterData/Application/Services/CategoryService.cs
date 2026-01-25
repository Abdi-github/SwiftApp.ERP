using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.MasterData.Application.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    ILogger<CategoryService> logger)
{
    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, ct);
        return category is null ? null : MapToResponse(category);
    }

    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetAllAsync(ct);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<List<CategoryResponse>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetRootCategoriesAsync(ct);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<List<CategoryResponse>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetChildrenAsync(parentId, ct);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
        };

        ApplyTranslations(category, request);
        await categoryRepository.AddAsync(category, ct);

        logger.LogInformation("Category {Name} created with id {CategoryId}", category.Name, category.Id);
        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;

        ApplyTranslations(category, request);
        await categoryRepository.UpdateAsync(category, ct);

        logger.LogInformation("Category {Name} updated", category.Name);
        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var children = await categoryRepository.GetChildrenAsync(id, ct);
        if (children.Count > 0)
            throw new BusinessRuleException("CATEGORY_HAS_CHILDREN", "Cannot delete a category that has child categories.");

        var category = await categoryRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Category), id);

        await categoryRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Category {Name} soft-deleted", category.Name);
    }

    private static void ApplyTranslations(Category category, CategoryRequest request)
    {
        category.Translations.Clear();

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
            category.Translations.Add(new CategoryTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? category.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }
    }

    private static CategoryResponse MapToResponse(Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.ParentCategoryId,
        category.ParentCategory?.Name,
        category.Translations.Count > 0
            ? category.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        category.Translations.Count > 0
            ? category.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null,
        category.CreatedAt,
        category.UpdatedAt);
}
