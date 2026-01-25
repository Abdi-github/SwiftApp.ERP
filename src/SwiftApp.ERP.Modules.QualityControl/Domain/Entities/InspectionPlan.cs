using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Entities;

public class InspectionPlan : BaseEntity
{
    public string PlanNumber { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? MaterialId { get; set; }

    public string? Criteria { get; set; }

    public string? Frequency { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<QualityCheck> QualityChecks { get; set; } = [];
}
