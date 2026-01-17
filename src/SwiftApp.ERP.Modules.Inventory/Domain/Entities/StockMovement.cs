using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Entities;

public class StockMovement : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;

    public MovementType MovementType { get; set; }

    public Guid ItemId { get; set; }

    public StockItemType ItemType { get; set; }

    public Guid? SourceWarehouseId { get; set; }

    public Warehouse? SourceWarehouse { get; set; }

    public Guid? TargetWarehouseId { get; set; }

    public Warehouse? TargetWarehouse { get; set; }

    public decimal Quantity { get; set; }

    public DateTimeOffset MovementDate { get; set; }

    public string? Reason { get; set; }

    public string? SourceDocumentType { get; set; }

    public Guid? SourceDocumentId { get; set; }
}
