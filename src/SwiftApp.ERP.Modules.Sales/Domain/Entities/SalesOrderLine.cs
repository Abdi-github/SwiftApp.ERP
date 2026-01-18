using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Domain.Entities;

public class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }

    public SalesOrder? SalesOrder { get; set; }

    public Guid ProductId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPct { get; set; }

    public VatRate VatRate { get; set; } = VatRate.Standard;

    public decimal LineTotal { get; set; }

    public int Position { get; set; }

    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice * (1 - DiscountPct / 100m);
    }

    public decimal GetVatAmount()
    {
        return LineTotal * VatRate.Multiplier();
    }
}
