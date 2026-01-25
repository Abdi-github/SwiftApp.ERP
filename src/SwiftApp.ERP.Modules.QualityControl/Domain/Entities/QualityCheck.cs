using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Entities;

public class QualityCheck : BaseEntity
{
    public string CheckNumber { get; set; } = string.Empty;

    public Guid? InspectionPlanId { get; set; }

    public InspectionPlan? InspectionPlan { get; set; }

    public string? InspectorName { get; set; }

    public DateOnly CheckDate { get; set; }

    public QualityCheckResult Result { get; set; }

    public Guid? ItemId { get; set; }

    public string? BatchNumber { get; set; }

    public int SampleSize { get; set; } = 1;

    public int DefectCount { get; set; }

    public string? Notes { get; set; }

    public ICollection<NonConformanceReport> NonConformanceReports { get; set; } = [];
}
