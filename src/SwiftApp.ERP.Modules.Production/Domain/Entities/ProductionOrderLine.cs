using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Entities;

public class ProductionOrderLine : BaseEntity
{
    public Guid ProductionOrderId { get; set; }

    public ProductionOrder? ProductionOrder { get; set; }

    public Guid MaterialId { get; set; }

    public string? Description { get; set; }

    public decimal RequiredQuantity { get; set; }

    public decimal IssuedQuantity { get; set; }

    public int Position { get; set; }
}
