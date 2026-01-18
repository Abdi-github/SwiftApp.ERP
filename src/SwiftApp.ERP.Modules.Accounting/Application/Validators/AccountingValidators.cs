using FluentValidation;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;

namespace SwiftApp.ERP.Modules.Accounting.Application.Validators;

public class AccountRequestValidator : AbstractValidator<AccountRequest>
{
    public AccountRequestValidator()
    {
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.AccountType)
            .NotEmpty()
            .Must(v => Enum.TryParse<AccountType>(v, ignoreCase: true, out _))
            .WithMessage("AccountType must be one of: Asset, Liability, Equity, Revenue, Expense.");
    }
}

public class JournalEntryRequestValidator : AbstractValidator<JournalEntryRequest>
{
    public JournalEntryRequestValidator()
    {
        RuleFor(x => x.EntryDate).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(5000);
        RuleFor(x => x.Reference).MaximumLength(255);
        RuleFor(x => x.SourceDocumentType).MaximumLength(50);
        RuleFor(x => x.Lines).NotNull().Must(l => l.Count >= 2)
            .WithMessage("A journal entry must have at least 2 lines.");
        RuleFor(x => x.Lines).Must(lines =>
        {
            var totalDebit = lines.Sum(l => l.Debit);
            var totalCredit = lines.Sum(l => l.Credit);
            return totalDebit == totalCredit;
        }).WithMessage("Total debits must equal total credits.");
        RuleForEach(x => x.Lines).SetValidator(new JournalEntryLineRequestValidator());
    }
}

public class JournalEntryLineRequestValidator : AbstractValidator<JournalEntryLineRequest>
{
    public JournalEntryLineRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Debit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Credit).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => (x.Debit > 0) != (x.Credit > 0))
            .WithMessage("Each line must have either a debit or a credit amount, not both and not neither.");
    }
}
