using FluentValidation;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Domain.Enums;

namespace SwiftApp.ERP.Modules.Crm.Application.Validators;

public class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.FirstName) || !string.IsNullOrWhiteSpace(x.LastName))
            .WithMessage("At least one of FirstName or LastName is required.");

        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Company).MaximumLength(255);
        RuleFor(x => x.Position).MaximumLength(255);
        RuleFor(x => x.Street).MaximumLength(500);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Canton).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(3);
        RuleFor(x => x.Notes).MaximumLength(5000);
    }
}

public class InteractionRequestValidator : AbstractValidator<InteractionRequest>
{
    public InteractionRequestValidator()
    {
        RuleFor(x => x.ContactId).NotEmpty();
        RuleFor(x => x.InteractionType)
            .NotEmpty()
            .Must(v => Enum.TryParse<InteractionType>(v, ignoreCase: true, out _))
            .WithMessage("InteractionType must be one of: Call, Email, Meeting, Note.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(5000);
    }
}
