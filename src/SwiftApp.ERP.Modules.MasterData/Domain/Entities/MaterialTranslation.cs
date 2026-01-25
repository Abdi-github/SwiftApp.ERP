using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class MaterialTranslation : BaseTranslation
{
    public Guid MaterialId { get; set; }

    public Material Material { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
