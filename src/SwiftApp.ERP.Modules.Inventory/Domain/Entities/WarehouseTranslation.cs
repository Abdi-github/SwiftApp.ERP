using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Entities;

public class WarehouseTranslation : BaseTranslation
{
    public Guid WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
