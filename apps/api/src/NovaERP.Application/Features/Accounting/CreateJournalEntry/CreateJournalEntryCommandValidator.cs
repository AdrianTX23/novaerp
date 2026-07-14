using FluentValidation;

namespace NovaERP.Application.Features.Accounting.CreateJournalEntry;

public sealed class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Reference).MaximumLength(100);
        RuleFor(x => x.Lines).NotEmpty().Must(l => l.Count >= 2)
            .WithMessage("Un asiento necesita al menos dos líneas.");
    }
}
