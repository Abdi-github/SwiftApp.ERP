using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    public ICollection<Category> Children { get; set; } = [];

    public ICollection<CategoryTranslation> Translations { get; set; } = [];
}
