using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class CategoryTranslation : BaseTranslation
{
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
