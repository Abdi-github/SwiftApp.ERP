using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Application.Services;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.Inventory.Tests;

public class StockServiceTests
{
    private readonly Mock<IStockLevelRepository> _stockLevelRepo = new();
    private readonly Mock<IStockMovementRepository> _movementRepo = new();
    private readonly Mock<IWarehouseRepository> _warehouseRepo = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<StockService>> _logger = new();
    private readonly StockService _sut;

    public StockServiceTests()
    {
        _sut = new StockService(_stockLevelRepo.Object, _movementRepo.Object, _warehouseRepo.Object, _publisher.Object, _logger.Object);
    }

    private static Warehouse CreateWarehouse(string code = "WH-ZH") => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Name = "Zürich Main",
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    // ── RecordMovementAsync ─────────────────────────────────────────────

    [Fact]
    public async Task ShouldRecordGoodsReceipt_AndAddStock()
    {
        var warehouse = CreateWarehouse();
        var itemId = Guid.NewGuid();
        var request = new StockMovementRequest("GoodsReceipt", itemId, "Product", null, warehouse.Id, 10m, "Initial stock");

        _warehouseRepo.Setup(r => r.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockLevel?)null);
        _movementRepo.Setup(r => r.CountTodayByTypeAsync(MovementType.GoodsReceipt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _sut.RecordMovementAsync(request);

        result.Should().NotBeNull();
        result.MovementType.Should().Be("GoodsReceipt");
        result.Quantity.Should().Be(10m);
        _stockLevelRepo.Verify(r => r.AddAsync(It.Is<StockLevel>(s => s.QuantityOnHand == 10m), It.IsAny<CancellationToken>()), Times.Once);
        _movementRepo.Verify(r => r.AddAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenInvalidMovementType()
    {
        var request = new StockMovementRequest("INVALID", Guid.NewGuid(), "Product", null, Guid.NewGuid(), 5m, null);

        var act = () => _sut.RecordMovementAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "INVALID_MOVEMENT_TYPE");
    }

    [Fact]
    public async Task ShouldThrow_WhenGoodsReceiptWithoutTargetWarehouse()
    {
        var request = new StockMovementRequest("GoodsReceipt", Guid.NewGuid(), "Product", null, null, 5m, null);

        var act = () => _sut.RecordMovementAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "TARGET_REQUIRED");
    }

    [Fact]
    public async Task ShouldThrow_WhenGoodsIssueWithInsufficientStock()
    {
        var warehouse = CreateWarehouse();
        var itemId = Guid.NewGuid();
        var request = new StockMovementRequest("GoodsIssue", itemId, "Product", warehouse.Id, null, 100m, null);

        _warehouseRepo.Setup(r => r.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockLevel { ItemId = itemId, WarehouseId = warehouse.Id, QuantityOnHand = 5m, QuantityReserved = 0m });

        var act = () => _sut.RecordMovementAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "INSUFFICIENT_STOCK");
    }

    [Fact]
    public async Task ShouldThrow_WhenTransferSameWarehouse()
    {
        var warehouseId = Guid.NewGuid();
        var request = new StockMovementRequest("Transfer", Guid.NewGuid(), "Product", warehouseId, warehouseId, 5m, null);

        var act = () => _sut.RecordMovementAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "SAME_WAREHOUSE");
    }

    // ── Stock queries ───────────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnStockLevel()
    {
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockLevel { QuantityOnHand = 25m, QuantityReserved = 5m });

        var result = await _sut.GetStockLevelAsync(itemId, warehouseId);

        result.Should().Be(20m); // 25 - 5
    }

    [Fact]
    public async Task ShouldReturnZero_WhenNoStockLevel()
    {
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(It.IsAny<Guid>(), It.IsAny<StockItemType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockLevel?)null);

        var result = await _sut.GetStockLevelAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnTrue_WhenStockAvailable()
    {
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockLevel { QuantityOnHand = 20m, QuantityReserved = 0m });

        var result = await _sut.IsStockAvailableAsync(itemId, warehouseId, 15m);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnFalse_WhenStockInsufficient()
    {
        var itemId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        _stockLevelRepo.Setup(r => r.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockLevel { QuantityOnHand = 5m, QuantityReserved = 0m });

        var result = await _sut.IsStockAvailableAsync(itemId, warehouseId, 10m);

        result.Should().BeFalse();
    }
}

public class WarehouseServiceTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepo = new();
    private readonly Mock<ILogger<WarehouseService>> _logger = new();
    private readonly WarehouseService _sut;

    public WarehouseServiceTests()
    {
        _sut = new WarehouseService(_warehouseRepo.Object, _logger.Object);
    }

    private static Warehouse CreateWarehouse(string code = "WH-ZH") => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Name = "Zürich Main",
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task ShouldCreateWarehouse_WhenCodeUnique()
    {
        var request = new WarehouseRequest("WH-BE", "Bern Warehouse", null, "Bern", true, null, null);
        _warehouseRepo.Setup(r => r.GetByCodeAsync("WH-BE", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var result = await _sut.CreateAsync(request);

        result.Code.Should().Be("WH-BE");
        result.Name.Should().Be("Bern Warehouse");
        _warehouseRepo.Verify(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenWarehouseCodeDuplicate()
    {
        _warehouseRepo.Setup(r => r.GetByCodeAsync("WH-ZH", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateWarehouse());

        var act = () => _sut.CreateAsync(new WarehouseRequest("WH-ZH", "Dup", null, null, true, null, null));

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "UNIQUE_CODE");
    }

    [Fact]
    public async Task ShouldSoftDelete_WhenFound()
    {
        var warehouse = CreateWarehouse();
        _warehouseRepo.Setup(r => r.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        await _sut.DeleteAsync(warehouse.Id);

        _warehouseRepo.Verify(r => r.SoftDeleteAsync(warehouse.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingNonExistentWarehouse()
    {
        _warehouseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
