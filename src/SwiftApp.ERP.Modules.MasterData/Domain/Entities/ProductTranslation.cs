using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class ProductTranslation : BaseTranslation
{
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
