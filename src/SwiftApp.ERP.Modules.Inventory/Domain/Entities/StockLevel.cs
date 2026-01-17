using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Entities;

public class StockLevel : BaseEntity
{
    public Guid ItemId { get; set; }

    public StockItemType ItemType { get; set; }

    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal QuantityReserved { get; set; }

    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
}
