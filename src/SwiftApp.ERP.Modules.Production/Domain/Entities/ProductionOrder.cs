using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Production.Domain.Entities;

public class ProductionOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid ProductId { get; set; }

    public Guid? WorkCenterId { get; set; }

    public WorkCenter? WorkCenter { get; set; }

    public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.Draft;

    public decimal PlannedQuantity { get; set; }

    public decimal CompletedQuantity { get; set; }

    public decimal ScrapQuantity { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }

    public DateOnly? ActualStartDate { get; set; }

    public DateOnly? ActualEndDate { get; set; }

    public decimal EstimatedCost { get; set; }

    public decimal ActualCost { get; set; }

    public string? Notes { get; set; }

    public ICollection<ProductionOrderLine> Lines { get; set; } = [];
}
