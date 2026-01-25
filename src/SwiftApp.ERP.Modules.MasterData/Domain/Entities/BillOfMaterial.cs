using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class BillOfMaterial : BaseEntity
{
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public Guid MaterialId { get; set; }

    public Material Material { get; set; } = null!;

    public decimal Quantity { get; set; }

    public Guid? UnitOfMeasureId { get; set; }

    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public int Position { get; set; }

    public string? Notes { get; set; }
}
