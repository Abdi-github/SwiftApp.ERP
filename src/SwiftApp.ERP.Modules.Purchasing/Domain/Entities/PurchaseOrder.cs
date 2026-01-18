using SwiftApp.ERP.Modules.Purchasing.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Purchasing.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? ExpectedDeliveryDate { get; set; }

    public DateOnly? ActualDeliveryDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "CHF";

    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];

    public void RecalculateTotals()
    {
        Subtotal = Lines.Sum(l => l.LineTotal);
        VatAmount = Lines.Sum(l => l.GetVatAmount());
        TotalAmount = Subtotal + VatAmount;
    }
}
