using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Production.Application.Services;

public class WorkCenterService(
    IWorkCenterRepository workCenterRepository,
    ILogger<WorkCenterService> logger)
{
    public async Task<WorkCenterResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var wc = await workCenterRepository.GetByIdAsync(id, ct);
        return wc is null ? null : MapToResponse(wc);
    }

    public async Task<WorkCenterResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var wc = await workCenterRepository.GetByCodeAsync(code, ct);
        return wc is null ? null : MapToResponse(wc);
    }

    public async Task<PagedResult<WorkCenterResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await workCenterRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<WorkCenterResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<WorkCenterResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var list = await workCenterRepository.GetAllActiveAsync(ct);
        return list.Select(MapToResponse).ToList();
    }

    public async Task<WorkCenterResponse> CreateAsync(WorkCenterRequest request, CancellationToken ct = default)
    {
        if (await workCenterRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Work center code '{request.Code}' already exists.");

        var wc = new WorkCenter
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            CapacityPerDay = request.CapacityPerDay ?? 1m,
            CostPerHour = request.CostPerHour ?? 0m,
            Active = request.Active ?? true,
        };

        ApplyTranslations(wc, request);
        await workCenterRepository.AddAsync(wc, ct);

        logger.LogInformation("Work center {Code} created with id {WorkCenterId}", wc.Code, wc.Id);

        return MapToResponse(wc);
    }

    public async Task<WorkCenterResponse> UpdateAsync(Guid id, WorkCenterRequest request, CancellationToken ct = default)
    {
        var wc = await workCenterRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(WorkCenter), id);

        if (wc.Code != request.Code && await workCenterRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Work center code '{request.Code}' already exists.");

        wc.Code = request.Code;
        wc.Name = request.Name;
        wc.Description = request.Description;
        wc.CapacityPerDay = request.CapacityPerDay ?? wc.CapacityPerDay;
        wc.CostPerHour = request.CostPerHour ?? wc.CostPerHour;
        wc.Active = request.Active ?? wc.Active;

        ApplyTranslations(wc, request);
        await workCenterRepository.UpdateAsync(wc, ct);

        logger.LogInformation("Work center {Code} updated", wc.Code);

        return MapToResponse(wc);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await workCenterRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(WorkCenter), id);

        await workCenterRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Work center {WorkCenterId} soft-deleted", id);
    }

    private static void ApplyTranslations(WorkCenter wc, WorkCenterRequest request)
    {
        wc.Translations.Clear();

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
            wc.Translations.Add(new WorkCenterTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? wc.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }
    }

    private static WorkCenterResponse MapToResponse(WorkCenter wc) => new(
        wc.Id,
        wc.Code,
        wc.Name,
        wc.Description,
        wc.CapacityPerDay,
        wc.CostPerHour,
        wc.Active,
        wc.CreatedAt,
        wc.UpdatedAt,
        wc.Translations.Count > 0
            ? wc.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        wc.Translations.Count > 0
            ? wc.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null);
}
