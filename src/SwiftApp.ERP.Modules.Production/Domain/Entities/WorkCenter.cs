using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Entities;

public class WorkCenter : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal CapacityPerDay { get; set; } = 1m;

    public decimal CostPerHour { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<WorkCenterTranslation> Translations { get; set; } = [];
}
