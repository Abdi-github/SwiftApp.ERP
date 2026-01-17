using FluentValidation;
using SwiftApp.ERP.Modules.Production.Application.DTOs;

namespace SwiftApp.ERP.Modules.Production.Application.Validators;

public class WorkCenterRequestValidator : AbstractValidator<WorkCenterRequest>
{
    public WorkCenterRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CapacityPerDay).GreaterThan(0).When(x => x.CapacityPerDay.HasValue);
        RuleFor(x => x.CostPerHour).GreaterThanOrEqualTo(0).When(x => x.CostPerHour.HasValue);
    }
}

public class ProductionOrderRequestValidator : AbstractValidator<ProductionOrderRequest>
{
    public ProductionOrderRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.PlannedQuantity).GreaterThan(0);
        RuleFor(x => x.PlannedStartDate).LessThanOrEqualTo(x => x.PlannedEndDate)
            .WithMessage("Planned start date must be on or before planned end date.");
        RuleForEach(x => x.Lines).SetValidator(new ProductionOrderLineRequestValidator())
            .When(x => x.Lines is { Count: > 0 });
    }
}

public class ProductionOrderLineRequestValidator : AbstractValidator<ProductionOrderLineRequest>
{
    public ProductionOrderLineRequestValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.RequiredQuantity).GreaterThan(0);
    }
}
