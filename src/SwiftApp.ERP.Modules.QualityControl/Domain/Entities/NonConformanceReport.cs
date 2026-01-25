using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.QualityControl.Domain.Entities;

public class NonConformanceReport : BaseEntity
{
    public string NcrNumber { get; set; } = string.Empty;

    public Guid? QualityCheckId { get; set; }

    public QualityCheck? QualityCheck { get; set; }

    public string? Description { get; set; }

    public NcrSeverity Severity { get; set; }

    public NcrStatus Status { get; set; } = NcrStatus.Open;

    public string? RootCause { get; set; }

    public string? CorrectiveAction { get; set; }

    public string? ResponsiblePerson { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly? ClosedDate { get; set; }
}
