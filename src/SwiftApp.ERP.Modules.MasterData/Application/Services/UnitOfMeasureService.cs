using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.Modules.MasterData.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.MasterData.Application.Services;

public class UnitOfMeasureService(
    IUnitOfMeasureRepository uomRepository,
    ILogger<UnitOfMeasureService> logger)
{
    public async Task<UnitOfMeasureResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var uom = await uomRepository.GetByIdAsync(id, ct);
        return uom is null ? null : MapToResponse(uom);
    }

    public async Task<UnitOfMeasureResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var uom = await uomRepository.GetByCodeAsync(code, ct);
        return uom is null ? null : MapToResponse(uom);
    }

    public async Task<List<UnitOfMeasureResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var uoms = await uomRepository.GetAllAsync(ct);
        return uoms.Select(MapToResponse).ToList();
    }

    public async Task<UnitOfMeasureResponse> CreateAsync(UnitOfMeasureRequest request, CancellationToken ct = default)
    {
        if (await uomRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"UoM code '{request.Code}' already exists.");

        var uom = new UnitOfMeasure
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
        };

        ApplyTranslations(uom, request);
        await uomRepository.AddAsync(uom, ct);

        logger.LogInformation("UnitOfMeasure {Code} created with id {UomId}", uom.Code, uom.Id);
        return MapToResponse(uom);
    }

    public async Task<UnitOfMeasureResponse> UpdateAsync(Guid id, UnitOfMeasureRequest request, CancellationToken ct = default)
    {
        var uom = await uomRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(UnitOfMeasure), id);

        if (uom.Code != request.Code && await uomRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"UoM code '{request.Code}' already exists.");

        uom.Code = request.Code;
        uom.Name = request.Name;
        uom.Description = request.Description;

        ApplyTranslations(uom, request);
        await uomRepository.UpdateAsync(uom, ct);

        logger.LogInformation("UnitOfMeasure {Code} updated", uom.Code);
        return MapToResponse(uom);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await uomRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("UnitOfMeasure {UomId} soft-deleted", id);
    }

    private static void ApplyTranslations(UnitOfMeasure uom, UnitOfMeasureRequest request)
    {
        uom.Translations.Clear();

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
            uom.Translations.Add(new UnitOfMeasureTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? uom.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }
    }

    private static UnitOfMeasureResponse MapToResponse(UnitOfMeasure uom) => new(
        uom.Id,
        uom.Code,
        uom.Name,
        uom.Description,
        uom.Translations.Count > 0
            ? uom.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        uom.Translations.Count > 0
            ? uom.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null,
        uom.CreatedAt,
        uom.UpdatedAt);
}
