using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Domain.Entities;

public class DepartmentTranslation : BaseTranslation
{
    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
