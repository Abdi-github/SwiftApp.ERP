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

public class NonConformanceReportService(
    INonConformanceReportRepository ncrRepository,
    IQualityCheckRepository checkRepository,
    IMediator mediator,
    ILogger<NonConformanceReportService> logger)
{
    public async Task<NcrResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrRepository.GetByIdAsync(id, ct);
        return ncr is null ? null : MapToResponse(ncr);
    }

    public async Task<NcrResponse?> GetByNcrNumberAsync(string ncrNumber, CancellationToken ct = default)
    {
        var ncr = await ncrRepository.GetByNcrNumberAsync(ncrNumber, ct);
        return ncr is null ? null : MapToResponse(ncr);
    }

    public async Task<PagedResult<NcrResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await ncrRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<NcrResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<NcrResponse>> GetByStatusAsync(NcrStatus status, CancellationToken ct = default)
    {
        var ncrs = await ncrRepository.GetByStatusAsync(status, ct);
        return ncrs.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<NcrResponse>> GetByQualityCheckAsync(Guid qualityCheckId, CancellationToken ct = default)
    {
        var ncrs = await ncrRepository.GetByQualityCheckAsync(qualityCheckId, ct);
        return ncrs.Select(MapToResponse).ToList();
    }

    public async Task<NcrResponse> CreateAsync(NcrRequest request, CancellationToken ct = default)
    {
        if (request.QualityCheckId.HasValue)
        {
            _ = await checkRepository.GetByIdAsync(request.QualityCheckId.Value, ct)
                ?? throw new BusinessRuleException("QUALITY_CHECK_NOT_FOUND",
                    $"Quality check with id '{request.QualityCheckId.Value}' does not exist.");
        }

        var severity = Enum.Parse<NcrSeverity>(request.Severity, ignoreCase: true);
        var ncrNumber = await ncrRepository.GetNextNcrNumberAsync(ct);

        var ncr = new NonConformanceReport
        {
            NcrNumber = ncrNumber,
            QualityCheckId = request.QualityCheckId,
            Description = request.Description,
            Severity = severity,
            Status = NcrStatus.Open,
            RootCause = request.RootCause,
            CorrectiveAction = request.CorrectiveAction,
            ResponsiblePerson = request.ResponsiblePerson,
            DueDate = request.DueDate,
        };

        await ncrRepository.AddAsync(ncr, ct);

        logger.LogInformation("NCR {NcrNumber} created with severity {Severity}",
            ncr.NcrNumber, ncr.Severity);

        await mediator.Publish(new NonConformanceReportCreatedEvent(
            ncr.Id, ncr.NcrNumber, ncr.Severity.ToString()), ct);

        // Reload to get QualityCheck navigation
        var created = await ncrRepository.GetByIdAsync(ncr.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<NcrResponse> UpdateAsync(Guid id, NcrRequest request, CancellationToken ct = default)
    {
        var ncr = await ncrRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(NonConformanceReport), id);

        if (request.QualityCheckId.HasValue)
        {
            _ = await checkRepository.GetByIdAsync(request.QualityCheckId.Value, ct)
                ?? throw new BusinessRuleException("QUALITY_CHECK_NOT_FOUND",
                    $"Quality check with id '{request.QualityCheckId.Value}' does not exist.");
        }

        var severity = Enum.Parse<NcrSeverity>(request.Severity, ignoreCase: true);

        ncr.QualityCheckId = request.QualityCheckId;
        ncr.Description = request.Description;
        ncr.Severity = severity;
        ncr.RootCause = request.RootCause;
        ncr.CorrectiveAction = request.CorrectiveAction;
        ncr.ResponsiblePerson = request.ResponsiblePerson;
        ncr.DueDate = request.DueDate;

        await ncrRepository.UpdateAsync(ncr, ct);

        logger.LogInformation("NCR {NcrNumber} updated", ncr.NcrNumber);

        return MapToResponse(ncr);
    }

    public async Task<NcrResponse> StartAsync(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(NonConformanceReport), id);

        if (ncr.Status is not NcrStatus.Open)
            throw new BusinessRuleException("NCR_INVALID_STATUS",
                $"NCR {ncr.NcrNumber} must be in Open status to start. Current status: {ncr.Status}.");

        ncr.Status = NcrStatus.InProgress;

        await ncrRepository.UpdateAsync(ncr, ct);

        logger.LogInformation("NCR {NcrNumber} moved to InProgress", ncr.NcrNumber);

        return MapToResponse(ncr);
    }

    public async Task<NcrResponse> CloseAsync(Guid id, CancellationToken ct = default)
    {
        var ncr = await ncrRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(NonConformanceReport), id);

        if (ncr.Status is not NcrStatus.InProgress)
            throw new BusinessRuleException("NCR_INVALID_STATUS",
                $"NCR {ncr.NcrNumber} must be in InProgress status to close. Current status: {ncr.Status}.");

        ncr.Status = NcrStatus.Closed;
        ncr.ClosedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await ncrRepository.UpdateAsync(ncr, ct);

        logger.LogInformation("NCR {NcrNumber} closed", ncr.NcrNumber);

        await mediator.Publish(new NonConformanceReportClosedEvent(
            ncr.Id, ncr.NcrNumber), ct);

        return MapToResponse(ncr);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await ncrRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(NonConformanceReport), id);

        await ncrRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("NCR {NcrId} soft-deleted", id);
    }

    private static NcrResponse MapToResponse(NonConformanceReport ncr) => new(
        ncr.Id,
        ncr.NcrNumber,
        ncr.QualityCheckId,
        ncr.QualityCheck?.CheckNumber,
        ncr.Description,
        ncr.Severity.ToString(),
        ncr.Status.ToString(),
        ncr.RootCause,
        ncr.CorrectiveAction,
        ncr.ResponsiblePerson,
        ncr.DueDate,
        ncr.ClosedDate,
        ncr.CreatedAt,
        ncr.UpdatedAt);
}
