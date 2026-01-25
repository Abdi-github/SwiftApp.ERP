using FluentValidation;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;

namespace SwiftApp.ERP.Modules.MasterData.Application.Validators;

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ListPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatRate).IsInEnum();
    }
}

public class MaterialRequestValidator : AbstractValidator<MaterialRequest>
{
    public MaterialRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatRate).IsInEnum();
    }
}

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class UnitOfMeasureRequestValidator : AbstractValidator<UnitOfMeasureRequest>
{
    public UnitOfMeasureRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class BillOfMaterialRequestValidator : AbstractValidator<BillOfMaterialRequest>
{
    public BillOfMaterialRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
