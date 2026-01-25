using FluentValidation;
using SwiftApp.ERP.Modules.QualityControl.Application.DTOs;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;

namespace SwiftApp.ERP.Modules.QualityControl.Application.Validators;

public class InspectionPlanRequestValidator : AbstractValidator<InspectionPlanRequest>
{
    public InspectionPlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.Criteria).MaximumLength(5000);
        RuleFor(x => x.Frequency).MaximumLength(100);
    }
}

public class QualityCheckRequestValidator : AbstractValidator<QualityCheckRequest>
{
    public QualityCheckRequestValidator()
    {
        RuleFor(x => x.CheckDate).NotEmpty();
        RuleFor(x => x.Result).NotEmpty()
            .Must(r => Enum.TryParse<QualityCheckResult>(r, ignoreCase: true, out _))
            .WithMessage("Result must be one of: Pass, Fail, Conditional.");
        RuleFor(x => x.InspectorName).MaximumLength(255);
        RuleFor(x => x.BatchNumber).MaximumLength(50);
        RuleFor(x => x.SampleSize).GreaterThan(0).When(x => x.SampleSize.HasValue);
        RuleFor(x => x.DefectCount).GreaterThanOrEqualTo(0).When(x => x.DefectCount.HasValue);
        RuleFor(x => x.Notes).MaximumLength(5000);
    }
}

public class NcrRequestValidator : AbstractValidator<NcrRequest>
{
    public NcrRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Severity).NotEmpty()
            .Must(s => Enum.TryParse<NcrSeverity>(s, ignoreCase: true, out _))
            .WithMessage("Severity must be one of: Minor, Major, Critical.");
        RuleFor(x => x.RootCause).MaximumLength(5000);
        RuleFor(x => x.CorrectiveAction).MaximumLength(5000);
        RuleFor(x => x.ResponsiblePerson).MaximumLength(255);
    }
}
