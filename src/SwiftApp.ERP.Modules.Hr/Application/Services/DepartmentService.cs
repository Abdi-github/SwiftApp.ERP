using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Hr.Application.Services;

public class DepartmentService(
    IDepartmentRepository departmentRepository,
    ILogger<DepartmentService> logger)
{
    public async Task<DepartmentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, ct);
        return department is null ? null : MapToResponse(department);
    }

    public async Task<DepartmentResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var department = await departmentRepository.GetByCodeAsync(code, ct);
        return department is null ? null : MapToResponse(department);
    }

    public async Task<PagedResult<DepartmentResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await departmentRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<DepartmentResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var departments = await departmentRepository.GetAllActiveAsync(ct);
        return departments.Select(MapToResponse).ToList();
    }

    public async Task<DepartmentResponse> CreateAsync(DepartmentRequest request, CancellationToken ct = default)
    {
        if (await departmentRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Department code '{request.Code}' already exists.");

        var department = new Department
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ManagerId = request.ManagerId,
            Active = request.Active ?? true,
        };

        ApplyTranslations(department, request);
        await departmentRepository.AddAsync(department, ct);

        logger.LogInformation("Department {Code} created with id {DepartmentId}", department.Code, department.Id);

        return MapToResponse(department);
    }

    public async Task<DepartmentResponse> UpdateAsync(Guid id, DepartmentRequest request, CancellationToken ct = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Department), id);

        if (department.Code != request.Code && await departmentRepository.GetByCodeAsync(request.Code, ct) is not null)
            throw new BusinessRuleException("UNIQUE_CODE", $"Department code '{request.Code}' already exists.");

        department.Code = request.Code;
        department.Name = request.Name;
        department.Description = request.Description;
        department.ManagerId = request.ManagerId;
        department.Active = request.Active ?? department.Active;

        ApplyTranslations(department, request);
        await departmentRepository.UpdateAsync(department, ct);

        logger.LogInformation("Department {Code} updated", department.Code);

        return MapToResponse(department);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Department), id);

        if (department.Employees.Count > 0)
            throw new BusinessRuleException("DEPARTMENT_HAS_EMPLOYEES", $"Cannot delete department '{department.Code}' because it has {department.Employees.Count} employees.");

        await departmentRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Department {DepartmentId} soft-deleted", id);
    }

    private static void ApplyTranslations(Department department, DepartmentRequest request)
    {
        department.Translations.Clear();

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
            department.Translations.Add(new DepartmentTranslation
            {
                Locale = locale,
                Name = request.NameTranslations?.GetValueOrDefault(locale) ?? department.Name,
                Description = request.DescriptionTranslations?.GetValueOrDefault(locale),
            });
        }
    }

    private static DepartmentResponse MapToResponse(Department department) => new(
        department.Id,
        department.Code,
        department.Name,
        department.Description,
        department.ManagerId,
        department.Manager?.DisplayName,
        department.Active,
        department.Employees.Count,
        department.CreatedAt,
        department.UpdatedAt,
        department.Translations.Count > 0
            ? department.Translations.ToDictionary(t => t.Locale, t => t.Name)
            : null,
        department.Translations.Count > 0
            ? department.Translations
                .Where(t => t.Description is not null)
                .ToDictionary(t => t.Locale, t => t.Description!)
            : null);
}
