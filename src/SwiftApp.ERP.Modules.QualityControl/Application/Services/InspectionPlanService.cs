using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.QualityControl.Application.Services;

public class InspectionPlanService(
    IInspectionPlanRepository planRepository,
    ILogger<InspectionPlanService> logger)
{
    public async Task<InspectionPlanResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await planRepository.GetByIdAsync(id, ct);
        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<InspectionPlanResponse?> GetByPlanNumberAsync(string planNumber, CancellationToken ct = default)
    {
        var plan = await planRepository.GetByPlanNumberAsync(planNumber, ct);
        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<PagedResult<InspectionPlanResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await planRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<InspectionPlanResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<InspectionPlanResponse>> GetActiveAsync(CancellationToken ct = default)
    {
        var plans = await planRepository.GetActiveAsync(ct);
        return plans.Select(MapToResponse).ToList();
    }

    public async Task<InspectionPlanResponse> CreateAsync(InspectionPlanRequest request, CancellationToken ct = default)
    {
        var planNumber = await planRepository.GetNextPlanNumberAsync(ct);

        var plan = new InspectionPlan
        {
            PlanNumber = planNumber,
            Name = request.Name,
            Description = request.Description,
            ProductId = request.ProductId,
            MaterialId = request.MaterialId,
            Criteria = request.Criteria,
            Frequency = request.Frequency,
            Active = request.Active ?? true,
        };

        await planRepository.AddAsync(plan, ct);

        logger.LogInformation("Inspection plan {PlanNumber} created", plan.PlanNumber);

        return MapToResponse(plan);
    }

    public async Task<InspectionPlanResponse> UpdateAsync(Guid id, InspectionPlanRequest request, CancellationToken ct = default)
    {
        var plan = await planRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(InspectionPlan), id);

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.ProductId = request.ProductId;
        plan.MaterialId = request.MaterialId;
        plan.Criteria = request.Criteria;
        plan.Frequency = request.Frequency;
        plan.Active = request.Active ?? plan.Active;

        await planRepository.UpdateAsync(plan, ct);

        logger.LogInformation("Inspection plan {PlanNumber} updated", plan.PlanNumber);

        return MapToResponse(plan);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await planRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(InspectionPlan), id);

        await planRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Inspection plan {PlanId} soft-deleted", id);
    }

    private static InspectionPlanResponse MapToResponse(InspectionPlan plan) => new(
        plan.Id,
        plan.PlanNumber,
        plan.Name,
        plan.Description,
        plan.ProductId,
        plan.MaterialId,
        plan.Criteria,
        plan.Frequency,
        plan.Active,
        plan.CreatedAt,
        plan.UpdatedAt);
}
