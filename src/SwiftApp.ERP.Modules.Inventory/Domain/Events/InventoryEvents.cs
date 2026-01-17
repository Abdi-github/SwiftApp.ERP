using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Events;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Events;

public record StockMovementRecordedEvent(
    Guid MovementId,
    string ReferenceNumber,
    MovementType MovementType,
    Guid ItemId,
    StockItemType ItemType,
    Guid? SourceWarehouseId,
    Guid? TargetWarehouseId,
    decimal Quantity) : IDomainEvent;

public record LowStockAlertEvent(
    Guid ItemId,
    StockItemType ItemType,
    Guid WarehouseId,
    decimal CurrentQuantity,
    decimal MinimumThreshold) : IDomainEvent;
