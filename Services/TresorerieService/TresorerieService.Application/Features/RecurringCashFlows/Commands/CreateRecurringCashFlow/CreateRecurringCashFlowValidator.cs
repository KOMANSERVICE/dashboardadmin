namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.CreateRecurringCashFlow;

public class CreateRecurringCashFlowValidator : AbstractValidator<CreateRecurringCashFlowCommand>
{
    public CreateRecurringCashFlowValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de flux est invalide")
            .Must(t => t == CashFlowType.INCOME || t == CashFlowType.EXPENSE)
            .WithMessage("Le type de flux doit etre INCOME ou EXPENSE");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("L'identifiant de la categorie est obligatoire");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Le libelle est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le libelle ne peut pas depasser 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas depasser 1000 caracteres")
            .When(x => x.Description != null);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Le montant doit etre superieur a 0");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("L'identifiant du compte est obligatoire");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Le mode de paiement est obligatoire")
            .MaximumLength(50)
            .WithMessage("Le mode de paiement ne peut pas depasser 50 caracteres");

        RuleFor(x => x.Frequency)
            .IsInEnum()
            .WithMessage("La frequence est invalide");

        RuleFor(x => x.Interval)
            .GreaterThanOrEqualTo(1)
            .WithMessage("L'intervalle doit etre superieur ou egal a 1");

        // DayOfMonth obligatoire pour MONTHLY
        RuleFor(x => x.DayOfMonth)
            .NotNull()
            .WithMessage("Le jour du mois est obligatoire pour une frequence MONTHLY")
            .When(x => x.Frequency == RecurringFrequency.MONTHLY);

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 31)
            .WithMessage("Le jour du mois doit etre entre 1 et 31")
            .When(x => x.DayOfMonth.HasValue);

        // DayOfWeek obligatoire pour WEEKLY
        RuleFor(x => x.DayOfWeek)
            .NotNull()
            .WithMessage("Le jour de la semaine est obligatoire pour une frequence WEEKLY")
            .When(x => x.Frequency == RecurringFrequency.WEEKLY);

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(1, 7)
            .WithMessage("Le jour de la semaine doit etre entre 1 et 7")
            .When(x => x.DayOfWeek.HasValue);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("La date de debut est obligatoire");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("La date de fin doit etre posterieure a la date de debut")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("L'identifiant du createur est obligatoire");

        RuleFor(x => x.ThirdPartyName)
            .MaximumLength(200)
            .WithMessage("Le nom du tiers ne peut pas depasser 200 caracteres")
            .When(x => x.ThirdPartyName != null);
    }
}
