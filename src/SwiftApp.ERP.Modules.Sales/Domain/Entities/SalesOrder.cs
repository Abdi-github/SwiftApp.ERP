using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Sales.Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? DeliveryDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "CHF";

    public string? Notes { get; set; }

    public string? ShippingStreet { get; set; }

    public string? ShippingCity { get; set; }

    public string? ShippingPostalCode { get; set; }

    public string? ShippingCanton { get; set; }

    public string? ShippingCountry { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = [];

    public void RecalculateTotals()
    {
        Subtotal = Lines.Sum(l => l.LineTotal);
        VatAmount = Lines.Sum(l => l.GetVatAmount());
        TotalAmount = Subtotal + VatAmount;
    }
}
