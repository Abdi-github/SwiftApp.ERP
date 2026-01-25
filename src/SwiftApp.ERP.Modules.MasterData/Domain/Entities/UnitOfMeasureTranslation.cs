using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class UnitOfMeasureTranslation : BaseTranslation
{
    public Guid UnitOfMeasureId { get; set; }

    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
