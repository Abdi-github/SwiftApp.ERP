using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Inventory.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<WarehouseTranslation> Translations { get; set; } = [];
}
