using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Entities;

public class WorkCenterTranslation : BaseTranslation
{
    public Guid WorkCenterId { get; set; }

    public WorkCenter? WorkCenter { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
