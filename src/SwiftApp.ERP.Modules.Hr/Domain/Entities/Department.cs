using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Hr.Domain.Entities;

public class Department : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ManagerId { get; set; }

    public Employee? Manager { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<DepartmentTranslation> Translations { get; set; } = [];

    public ICollection<Employee> Employees { get; set; } = [];
}
