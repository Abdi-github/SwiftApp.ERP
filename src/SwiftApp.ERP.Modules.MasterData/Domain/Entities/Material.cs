using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class Material : BaseEntity
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public Category? Category { get; set; }

    public Guid? UnitOfMeasureId { get; set; }

    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public decimal UnitPrice { get; set; }

    public VatRate VatRate { get; set; } = VatRate.Standard;

    public decimal MinimumStock { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<MaterialTranslation> Translations { get; set; } = [];
}
