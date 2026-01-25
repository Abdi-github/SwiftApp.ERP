using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class UnitOfMeasure : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UnitOfMeasureTranslation> Translations { get; set; } = [];
}
