using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Application.Services;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.Modules.Production.Domain.Events;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.Production.Tests;

public class ProductionOrderServiceTests
{
    private readonly Mock<IProductionOrderRepository> _orderRepo = new();
    private readonly Mock<IProductionOrderLineRepository> _lineRepo = new();
    private readonly Mock<IWorkCenterRepository> _workCenterRepo = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<ProductionOrderService>> _logger = new();
    private readonly ProductionOrderService _sut;

    public ProductionOrderServiceTests()
    {
        _sut = new ProductionOrderService(_orderRepo.Object, _lineRepo.Object, _workCenterRepo.Object, _publisher.Object, _logger.Object);
    }

    private static ProductionOrder CreateTestOrder(ProductionOrderStatus status = ProductionOrderStatus.Draft) => new()
    {
        Id = Guid.NewGuid(),
        OrderNumber = "MO-2026-00001",
        ProductId = Guid.NewGuid(),
        Status = status,
        PlannedQuantity = 100m,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ShouldCreateOrder_WithGeneratedNumber()
    {
        var request = new ProductionOrderRequest(Guid.NewGuid(), null, 50m,
            DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, null);
        _orderRepo.Setup(r => r.GetMaxSequenceForYearAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _sut.CreateAsync(request);

        result.OrderNumber.Should().StartWith("MO-");
        result.Status.Should().Be("Draft");
        _orderRepo.Verify(r => r.AddAsync(It.IsAny<ProductionOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<ProductionOrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReleaseOrder_WhenDraftWithLines()
    {
        var order = CreateTestOrder();
        order.Lines.Add(new ProductionOrderLine { MaterialId = Guid.NewGuid(), RequiredQuantity = 10m });
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.ReleaseAsync(order.Id);

        result.Status.Should().Be("Released");
        _publisher.Verify(p => p.Publish(It.IsAny<ProductionOrderReleasedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenReleasingNonDraftOrder()
    {
        var order = CreateTestOrder(ProductionOrderStatus.InProgress);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = () => _sut.ReleaseAsync(order.Id);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "ORDER_NOT_DRAFT");
    }

    [Fact]
    public async Task ShouldStartOrder_WhenReleased()
    {
        var order = CreateTestOrder(ProductionOrderStatus.Released);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.StartAsync(order.Id);

        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task ShouldCompleteOrder_WithQuantities()
    {
        var order = CreateTestOrder(ProductionOrderStatus.InProgress);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.CompleteAsync(order.Id, 95m, 5m);

        result.Status.Should().Be("Completed");
        result.CompletedQuantity.Should().Be(95m);
        result.ScrapQuantity.Should().Be(5m);
        _publisher.Verify(p => p.Publish(It.IsAny<ProductionOrderCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldPutOnHold_WhenReleasedOrInProgress()
    {
        var order = CreateTestOrder(ProductionOrderStatus.Released);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.HoldAsync(order.Id);

        result.Status.Should().Be("OnHold");
    }

    [Fact]
    public async Task ShouldResumeOrder_WhenOnHold()
    {
        var order = CreateTestOrder(ProductionOrderStatus.OnHold);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.ResumeAsync(order.Id);

        result.Status.Should().Be("Released");
    }

    [Fact]
    public async Task ShouldThrow_WhenCancellingInProgressOrder()
    {
        var order = CreateTestOrder(ProductionOrderStatus.InProgress);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = () => _sut.CancelAsync(order.Id, "reason");

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "ORDER_CANNOT_CANCEL");
    }

    [Theory]
    [InlineData(ProductionOrderStatus.Draft)]
    [InlineData(ProductionOrderStatus.Cancelled)]
    public async Task ShouldDeleteOrder_WhenDraftOrCancelled(ProductionOrderStatus status)
    {
        var order = CreateTestOrder(status);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _sut.DeleteAsync(order.Id);

        _orderRepo.Verify(r => r.SoftDeleteAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingActiveOrder()
    {
        var order = CreateTestOrder(ProductionOrderStatus.InProgress);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = () => _sut.DeleteAsync(order.Id);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "ORDER_CANNOT_DELETE");
    }
}
