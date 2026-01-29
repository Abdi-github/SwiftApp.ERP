using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Application.Services;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.Modules.QualityControl.Domain.Events;
using SwiftApp.ERP.Modules.QualityControl.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.QualityControl.Tests;

public class QualityCheckServiceTests
{
    private readonly Mock<IQualityCheckRepository> _checkRepo = new();
    private readonly Mock<IInspectionPlanRepository> _planRepo = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<QualityCheckService>> _logger = new();
    private readonly QualityCheckService _sut;

    public QualityCheckServiceTests()
    {
        _sut = new QualityCheckService(_checkRepo.Object, _planRepo.Object, _mediator.Object, _logger.Object);
    }

    private static InspectionPlan CreateInspectionPlan() => new()
    {
        Id = Guid.NewGuid(),
        PlanNumber = "IP-001",
        Name = "Final Assembly Inspection",
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static QualityCheck CreateQualityCheck(QualityCheckResult result = QualityCheckResult.Pass) => new()
    {
        Id = Guid.NewGuid(),
        CheckNumber = "QC-000001",
        InspectorName = "Pierre Blanc",
        CheckDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Result = result,
        SampleSize = 10,
        DefectCount = 0,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ShouldCreateQualityCheck_WithAutoNumber()
    {
        var plan = CreateInspectionPlan();
        var request = new QualityCheckRequest(plan.Id, "Pierre Blanc", DateOnly.FromDateTime(DateTime.UtcNow),
            "Pass", Guid.NewGuid(), "BATCH-2026-001", 10, 0, "All checks passed");

        _planRepo.Setup(r => r.GetByIdAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _checkRepo.Setup(r => r.GetNextCheckNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("QC-000001");
        _checkRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateQualityCheck());

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Result.Should().Be("Pass");
        _checkRepo.Verify(r => r.AddAsync(It.IsAny<QualityCheck>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediator.Verify(m => m.Publish(It.IsAny<QualityCheckCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldCreateQualityCheck_WithFail()
    {
        var request = new QualityCheckRequest(null, "Pierre Blanc", DateOnly.FromDateTime(DateTime.UtcNow),
            "Fail", Guid.NewGuid(), "BATCH-2026-002", 20, 5, "5 defects found");

        _checkRepo.Setup(r => r.GetNextCheckNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("QC-000002");
        _checkRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateQualityCheck(QualityCheckResult.Fail));

        var result = await _sut.CreateAsync(request);

        result.Result.Should().Be("Fail");
    }

    [Fact]
    public async Task ShouldThrow_WhenInspectionPlanNotFound()
    {
        var badPlanId = Guid.NewGuid();
        var request = new QualityCheckRequest(badPlanId, "Inspector", DateOnly.FromDateTime(DateTime.UtcNow),
            "Pass", null, null, null, null, null);

        _planRepo.Setup(r => r.GetByIdAsync(badPlanId, It.IsAny<CancellationToken>())).ReturnsAsync((InspectionPlan?)null);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "INSPECTION_PLAN_NOT_FOUND");
    }

    [Fact]
    public async Task ShouldReturnCheck_WhenFound()
    {
        var check = CreateQualityCheck();
        _checkRepo.Setup(r => r.GetByIdAsync(check.Id, It.IsAny<CancellationToken>())).ReturnsAsync(check);

        var result = await _sut.GetByIdAsync(check.Id);

        result.Should().NotBeNull();
        result!.CheckNumber.Should().Be("QC-000001");
    }

    [Fact]
    public async Task ShouldReturnNull_WhenCheckNotFound()
    {
        _checkRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((QualityCheck?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldUpdateCheck()
    {
        var check = CreateQualityCheck();
        _checkRepo.Setup(r => r.GetByIdAsync(check.Id, It.IsAny<CancellationToken>())).ReturnsAsync(check);

        var request = new QualityCheckRequest(null, "New Inspector", DateOnly.FromDateTime(DateTime.UtcNow),
            "Conditional", null, "BATCH-2026-003", 15, 2, "Conditional pass");

        var result = await _sut.UpdateAsync(check.Id, request);

        result.Result.Should().Be("Conditional");
        result.InspectorName.Should().Be("New Inspector");
        _checkRepo.Verify(r => r.UpdateAsync(It.IsAny<QualityCheck>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldSoftDelete_WhenCheckFound()
    {
        var check = CreateQualityCheck();
        _checkRepo.Setup(r => r.GetByIdAsync(check.Id, It.IsAny<CancellationToken>())).ReturnsAsync(check);

        await _sut.DeleteAsync(check.Id);

        _checkRepo.Verify(r => r.SoftDeleteAsync(check.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingNonExistentCheck()
    {
        _checkRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((QualityCheck?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
