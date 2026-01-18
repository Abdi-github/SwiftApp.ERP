using FluentValidation;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;

namespace SwiftApp.ERP.Modules.Purchasing.Application.Validators;

public class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.CompanyName) ||
                       (!string.IsNullOrWhiteSpace(x.FirstName) && !string.IsNullOrWhiteSpace(x.LastName)))
            .WithMessage("Either CompanyName or both FirstName and LastName must be provided.");

        RuleFor(x => x.CompanyName).MaximumLength(255);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Street).MaximumLength(500);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Canton).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(3);
        RuleFor(x => x.VatNumber).MaximumLength(30);
        RuleFor(x => x.PaymentTerms).GreaterThanOrEqualTo(0).When(x => x.PaymentTerms.HasValue);
        RuleFor(x => x.ContactPerson).MaximumLength(255);
        RuleFor(x => x.Website).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(5000);
    }
}

public class PurchaseOrderRequestValidator : AbstractValidator<PurchaseOrderRequest>
{
    public PurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(5000);
        RuleForEach(x => x.Lines).SetValidator(new PurchaseOrderLineRequestValidator())
            .When(x => x.Lines is not null);
    }
}

public class PurchaseOrderLineRequestValidator : AbstractValidator<PurchaseOrderLineRequest>
{
    public PurchaseOrderLineRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPct).InclusiveBetween(0, 100).When(x => x.DiscountPct.HasValue);
        RuleFor(x => x.VatRate).NotEmpty().MaximumLength(50);
    }
}
