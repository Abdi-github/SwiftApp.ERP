using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.Modules.QualityControl.Domain.Events;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.QualityControl.Application.Services;

public class QualityCheckService(
    IQualityCheckRepository checkRepository,
    IInspectionPlanRepository planRepository,
    IMediator mediator,
    ILogger<QualityCheckService> logger)
{
    public async Task<QualityCheckResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var check = await checkRepository.GetByIdAsync(id, ct);
        return check is null ? null : MapToResponse(check);
    }

    public async Task<QualityCheckResponse?> GetByCheckNumberAsync(string checkNumber, CancellationToken ct = default)
    {
        var check = await checkRepository.GetByCheckNumberAsync(checkNumber, ct);
        return check is null ? null : MapToResponse(check);
    }

    public async Task<PagedResult<QualityCheckResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await checkRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<QualityCheckResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<QualityCheckResponse>> GetByInspectionPlanAsync(Guid inspectionPlanId, CancellationToken ct = default)
    {
        var checks = await checkRepository.GetByInspectionPlanAsync(inspectionPlanId, ct);
        return checks.Select(MapToResponse).ToList();
    }

    public async Task<QualityCheckResponse> CreateAsync(QualityCheckRequest request, CancellationToken ct = default)
    {
        if (request.InspectionPlanId.HasValue)
        {
            _ = await planRepository.GetByIdAsync(request.InspectionPlanId.Value, ct)
                ?? throw new BusinessRuleException("INSPECTION_PLAN_NOT_FOUND",
                    $"Inspection plan with id '{request.InspectionPlanId.Value}' does not exist.");
        }

        var result = Enum.Parse<QualityCheckResult>(request.Result, ignoreCase: true);
        var checkNumber = await checkRepository.GetNextCheckNumberAsync(ct);

        var check = new QualityCheck
        {
            CheckNumber = checkNumber,
            InspectionPlanId = request.InspectionPlanId,
            InspectorName = request.InspectorName,
            CheckDate = request.CheckDate,
            Result = result,
            ItemId = request.ItemId,
            BatchNumber = request.BatchNumber,
            SampleSize = request.SampleSize ?? 1,
            DefectCount = request.DefectCount ?? 0,
            Notes = request.Notes,
        };

        await checkRepository.AddAsync(check, ct);

        logger.LogInformation("Quality check {CheckNumber} completed with result {Result}",
            check.CheckNumber, check.Result);

        await mediator.Publish(new QualityCheckCompletedEvent(
            check.Id, check.CheckNumber, check.Result.ToString()), ct);

        // Reload to get InspectionPlan navigation
        var created = await checkRepository.GetByIdAsync(check.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<QualityCheckResponse> UpdateAsync(Guid id, QualityCheckRequest request, CancellationToken ct = default)
    {
        var check = await checkRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(QualityCheck), id);

        if (request.InspectionPlanId.HasValue)
        {
            _ = await planRepository.GetByIdAsync(request.InspectionPlanId.Value, ct)
                ?? throw new BusinessRuleException("INSPECTION_PLAN_NOT_FOUND",
                    $"Inspection plan with id '{request.InspectionPlanId.Value}' does not exist.");
        }

        var result = Enum.Parse<QualityCheckResult>(request.Result, ignoreCase: true);

        check.InspectionPlanId = request.InspectionPlanId;
        check.InspectorName = request.InspectorName;
        check.CheckDate = request.CheckDate;
        check.Result = result;
        check.ItemId = request.ItemId;
        check.BatchNumber = request.BatchNumber;
        check.SampleSize = request.SampleSize ?? check.SampleSize;
        check.DefectCount = request.DefectCount ?? check.DefectCount;
        check.Notes = request.Notes;

        await checkRepository.UpdateAsync(check, ct);

        logger.LogInformation("Quality check {CheckNumber} updated", check.CheckNumber);

        return MapToResponse(check);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await checkRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(QualityCheck), id);

        await checkRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Quality check {CheckId} soft-deleted", id);
    }

    private static QualityCheckResponse MapToResponse(QualityCheck check) => new(
        check.Id,
        check.CheckNumber,
        check.InspectionPlanId,
        check.InspectionPlan?.PlanNumber,
        check.InspectorName,
        check.CheckDate,
        check.Result.ToString(),
        check.ItemId,
        check.BatchNumber,
        check.SampleSize,
        check.DefectCount,
        check.Notes,
        check.CreatedAt,
        check.UpdatedAt);
}
