using FluentValidation;
using SwiftApp.ERP.Modules.Notification.Application.DTOs;

namespace SwiftApp.ERP.Modules.Notification.Application.Validators;

public class MailCampaignRequestValidator : AbstractValidator<MailCampaignRequest>
{
    public MailCampaignRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TemplateCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Locale).MaximumLength(10);
        RuleFor(x => x.TargetSegment).MaximumLength(100);
        RuleFor(x => x.SubjectOverride).MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
