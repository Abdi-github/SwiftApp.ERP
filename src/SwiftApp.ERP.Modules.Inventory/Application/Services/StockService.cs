using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Inventory.Application.DTOs;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.Modules.Inventory.Domain.Events;
using SwiftApp.ERP.Modules.Inventory.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Inventory.Application.Services;

public class StockService(
    IStockLevelRepository stockLevelRepository,
    IStockMovementRepository stockMovementRepository,
    IWarehouseRepository warehouseRepository,
    IPublisher publisher,
    ILogger<StockService> logger)
{
    private static readonly Dictionary<MovementType, string> MovementPrefixes = new()
    {
        [MovementType.GoodsReceipt] = "GR",
        [MovementType.GoodsIssue] = "GI",
        [MovementType.ProductionIssue] = "PI",
        [MovementType.ProductionReceipt] = "PR",
        [MovementType.Shipment] = "SH",
        [MovementType.Transfer] = "TR",
        [MovementType.Adjustment] = "AD",
        [MovementType.Return] = "RT",
        [MovementType.Scrap] = "SC",
    };

    public async Task<StockMovementResponse> RecordMovementAsync(StockMovementRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<MovementType>(request.MovementType, ignoreCase: true, out var movementType))
            throw new BusinessRuleException("INVALID_MOVEMENT_TYPE", $"Invalid movement type: '{request.MovementType}'.");

        if (!Enum.TryParse<StockItemType>(request.ItemType, ignoreCase: true, out var itemType))
            throw new BusinessRuleException("INVALID_ITEM_TYPE", $"Invalid item type: '{request.ItemType}'.");

        ValidateWarehouses(movementType, request.SourceWarehouseId, request.TargetWarehouseId);
        // logger.LogDebug("RecordMovement input: Type={MovementType}, ItemType={ItemType}, Qty={Quantity}, Source={Source}, Target={Target}", movementType, itemType, request.Quantity, request.SourceWarehouseId, request.TargetWarehouseId);

        Warehouse? sourceWarehouse = null;
        Warehouse? targetWarehouse = null;

        if (request.SourceWarehouseId.HasValue)
        {
            sourceWarehouse = await warehouseRepository.GetByIdAsync(request.SourceWarehouseId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(Warehouse), request.SourceWarehouseId.Value);
        }

        if (request.TargetWarehouseId.HasValue)
        {
            targetWarehouse = await warehouseRepository.GetByIdAsync(request.TargetWarehouseId.Value, ct)
                ?? throw new EntityNotFoundException(nameof(Warehouse), request.TargetWarehouseId.Value);
        }

        // Process stock changes based on movement type
        switch (movementType)
        {
            case MovementType.GoodsReceipt:
            case MovementType.ProductionReceipt:
            case MovementType.Return:
                await AddStockAsync(request.ItemId, itemType, request.TargetWarehouseId!.Value, request.Quantity, ct);
                break;

            case MovementType.GoodsIssue:
            case MovementType.ProductionIssue:
            case MovementType.Shipment:
            case MovementType.Scrap:
                await RemoveStockAsync(request.ItemId, itemType, request.SourceWarehouseId!.Value, request.Quantity, ct);
                break;

            case MovementType.Transfer:
                await RemoveStockAsync(request.ItemId, itemType, request.SourceWarehouseId!.Value, request.Quantity, ct);
                await AddStockAsync(request.ItemId, itemType, request.TargetWarehouseId!.Value, request.Quantity, ct);
                break;

            case MovementType.Adjustment:
                var warehouseId = request.TargetWarehouseId ?? request.SourceWarehouseId
                    ?? throw new BusinessRuleException("WAREHOUSE_REQUIRED", "Adjustment requires a warehouse.");
                await AdjustStockAsync(request.ItemId, itemType, warehouseId, request.Quantity, ct);
                break;
        }

        var referenceNumber = await GenerateReferenceNumberAsync(movementType, ct);
        var movement = new StockMovement
        {
            ReferenceNumber = referenceNumber,
            MovementType = movementType,
            ItemId = request.ItemId,
            ItemType = itemType,
            SourceWarehouseId = request.SourceWarehouseId,
            TargetWarehouseId = request.TargetWarehouseId,
            Quantity = request.Quantity,
            MovementDate = DateTimeOffset.UtcNow,
            Reason = request.Reason,
        };

        await stockMovementRepository.AddAsync(movement, ct);
        // System.Diagnostics.Debug.WriteLine($"Movement persisted with reference={movement.ReferenceNumber}, id={movement.Id}");

        logger.LogInformation("Stock movement {ReferenceNumber} recorded: {MovementType} {Quantity} of {ItemType} {ItemId}",
            referenceNumber, movementType, request.Quantity, itemType, request.ItemId);

        await publisher.Publish(new StockMovementRecordedEvent(
            movement.Id, movement.ReferenceNumber, movement.MovementType,
            movement.ItemId, movement.ItemType,
            movement.SourceWarehouseId, movement.TargetWarehouseId,
            movement.Quantity), ct);

        return MapToMovementResponse(movement, sourceWarehouse, targetWarehouse);
    }

    public async Task<PagedResult<StockLevelResponse>> GetStockLevelsPagedAsync(int page, int size, CancellationToken ct = default)
    {
        var result = await stockLevelRepository.GetPagedAsync(page, size, ct);
        var items = result.Items.Select(l => MapToStockLevelResponse(l)).ToList();
        return new PagedResult<StockLevelResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<StockLevelResponse>> GetStockLevelsByItemAsync(Guid itemId, string itemType, CancellationToken ct = default)
    {
        if (!Enum.TryParse<StockItemType>(itemType, ignoreCase: true, out var parsedType))
            throw new BusinessRuleException("INVALID_ITEM_TYPE", $"Invalid item type: '{itemType}'.");

        var levels = await stockLevelRepository.GetByItemAsync(itemId, parsedType, ct);
        return levels.Select(l => MapToStockLevelResponse(l)).ToList();
    }

    public async Task<PagedResult<StockMovementResponse>> GetMovementsPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await stockMovementRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(m => MapToMovementResponse(m, m.SourceWarehouse, m.TargetWarehouse)).ToList();
        return new PagedResult<StockMovementResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<StockMovementResponse?> GetMovementByIdAsync(Guid id, CancellationToken ct = default)
    {
        var movement = await stockMovementRepository.GetByIdAsync(id, ct);
        return movement is null ? null : MapToMovementResponse(movement, movement.SourceWarehouse, movement.TargetWarehouse);
    }

    public async Task<decimal> GetStockLevelAsync(Guid itemId, Guid warehouseId, CancellationToken ct = default)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, ct);
        return level?.QuantityAvailable ?? 0m;
    }

    public async Task<decimal> GetTotalStockLevelAsync(Guid itemId, CancellationToken ct = default)
    {
        var levels = await stockLevelRepository.GetByItemAsync(itemId, StockItemType.Product, ct);
        return levels.Sum(l => l.QuantityAvailable);
    }

    public async Task<bool> IsStockAvailableAsync(Guid itemId, Guid warehouseId, decimal quantity, CancellationToken ct = default)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, StockItemType.Product, warehouseId, ct);
        return level is not null && level.QuantityAvailable >= quantity;
    }

    private static void ValidateWarehouses(MovementType type, Guid? sourceId, Guid? targetId)
    {
        switch (type)
        {
            case MovementType.GoodsReceipt:
            case MovementType.ProductionReceipt:
            case MovementType.Return:
                if (!targetId.HasValue)
                    throw new BusinessRuleException("TARGET_REQUIRED", $"Target warehouse is required for {type}.");
                break;

            case MovementType.GoodsIssue:
            case MovementType.ProductionIssue:
            case MovementType.Shipment:
            case MovementType.Scrap:
                if (!sourceId.HasValue)
                    throw new BusinessRuleException("SOURCE_REQUIRED", $"Source warehouse is required for {type}.");
                break;

            case MovementType.Transfer:
                if (!sourceId.HasValue || !targetId.HasValue)
                    throw new BusinessRuleException("BOTH_WAREHOUSES_REQUIRED", "Transfer requires both source and target warehouses.");
                if (sourceId == targetId)
                    throw new BusinessRuleException("SAME_WAREHOUSE", "Source and target warehouses must be different for transfers.");
                break;

            case MovementType.Adjustment:
                if (!sourceId.HasValue && !targetId.HasValue)
                    throw new BusinessRuleException("WAREHOUSE_REQUIRED", "Adjustment requires at least one warehouse.");
                break;
        }
    }

    private async Task AddStockAsync(Guid itemId, StockItemType itemType, Guid warehouseId, decimal quantity, CancellationToken ct)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, itemType, warehouseId, ct);
        if (level is null)
        {
            level = new StockLevel
            {
                ItemId = itemId,
                ItemType = itemType,
                WarehouseId = warehouseId,
                QuantityOnHand = quantity,
                QuantityReserved = 0m,
            };
            await stockLevelRepository.AddAsync(level, ct);
        }
        else
        {
            level.QuantityOnHand += quantity;
            await stockLevelRepository.UpdateAsync(level, ct);
        }
    }

    private async Task RemoveStockAsync(Guid itemId, StockItemType itemType, Guid warehouseId, decimal quantity, CancellationToken ct)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, itemType, warehouseId, ct)
            ?? throw new BusinessRuleException("NO_STOCK", $"No stock found for item {itemId} in warehouse {warehouseId}.");

        if (level.QuantityAvailable < quantity)
            throw new BusinessRuleException("INSUFFICIENT_STOCK",
                $"Insufficient stock. Available: {level.QuantityAvailable}, requested: {quantity}.");

        level.QuantityOnHand -= quantity;
        await stockLevelRepository.UpdateAsync(level, ct);
    }

    private async Task AdjustStockAsync(Guid itemId, StockItemType itemType, Guid warehouseId, decimal quantity, CancellationToken ct)
    {
        var level = await stockLevelRepository.GetByItemAndWarehouseAsync(itemId, itemType, warehouseId, ct);
        if (level is null)
        {
            level = new StockLevel
            {
                ItemId = itemId,
                ItemType = itemType,
                WarehouseId = warehouseId,
                QuantityOnHand = quantity,
                QuantityReserved = 0m,
            };
            await stockLevelRepository.AddAsync(level, ct);
        }
        else
        {
            level.QuantityOnHand = quantity;
            await stockLevelRepository.UpdateAsync(level, ct);
        }
    }

    private async Task<string> GenerateReferenceNumberAsync(MovementType type, CancellationToken ct)
    {
        var prefix = MovementPrefixes[type];
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var count = await stockMovementRepository.CountTodayByTypeAsync(type, ct);
        return $"{prefix}-{dateStr}-{count + 1:D4}";
    }

    private static StockLevelResponse MapToStockLevelResponse(StockLevel level, string itemName = "") => new(
        level.Id,
        level.ItemId,
        level.ItemType.ToString(),
        itemName,
        level.WarehouseId,
        level.Warehouse?.Code ?? string.Empty,
        level.Warehouse?.Name ?? string.Empty,
        level.QuantityOnHand,
        level.QuantityReserved,
        level.QuantityAvailable);

    private static StockMovementResponse MapToMovementResponse(StockMovement movement, Warehouse? source, Warehouse? target) => new(
        movement.Id,
        movement.ReferenceNumber,
        movement.MovementType.ToString(),
        movement.ItemId,
        movement.ItemType.ToString(),
        movement.SourceWarehouseId,
        source?.Code,
        movement.TargetWarehouseId,
        target?.Code,
        movement.Quantity,
        movement.MovementDate,
        movement.Reason,
        movement.SourceDocumentType,
        movement.SourceDocumentId,
        movement.CreatedAt);
}
