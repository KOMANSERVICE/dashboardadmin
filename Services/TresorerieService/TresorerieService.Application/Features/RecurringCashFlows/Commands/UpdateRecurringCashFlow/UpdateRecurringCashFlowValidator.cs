namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.UpdateRecurringCashFlow;

public class UpdateRecurringCashFlowValidator : AbstractValidator<UpdateRecurringCashFlowCommand>
{
    public UpdateRecurringCashFlowValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("L'identifiant du flux recurrent est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Data)
            .NotNull()
            .WithMessage("Les donnees de mise a jour sont obligatoires");

        When(x => x.Data != null, () =>
        {
            // Validation du type si fourni
            RuleFor(x => x.Data.Type)
                .IsInEnum()
                .WithMessage("Le type de flux est invalide")
                .Must(t => !t.HasValue || t == CashFlowType.INCOME || t == CashFlowType.EXPENSE)
                .WithMessage("Le type de flux doit etre INCOME ou EXPENSE")
                .When(x => x.Data.Type.HasValue);

            // Validation du label si fourni
            RuleFor(x => x.Data.Label)
                .MaximumLength(200)
                .WithMessage("Le libelle ne peut pas depasser 200 caracteres")
                .When(x => x.Data.Label != null);

            // Validation de la description si fournie
            RuleFor(x => x.Data.Description)
                .MaximumLength(1000)
                .WithMessage("La description ne peut pas depasser 1000 caracteres")
                .When(x => x.Data.Description != null);

            // Validation du montant si fourni
            RuleFor(x => x.Data.Amount)
                .GreaterThan(0)
                .WithMessage("Le montant doit etre superieur a 0")
                .When(x => x.Data.Amount.HasValue);

            // Validation du mode de paiement si fourni
            RuleFor(x => x.Data.PaymentMethod)
                .MaximumLength(50)
                .WithMessage("Le mode de paiement ne peut pas depasser 50 caracteres")
                .When(x => x.Data.PaymentMethod != null);

            // Validation de la frequence si fournie
            RuleFor(x => x.Data.Frequency)
                .IsInEnum()
                .WithMessage("La frequence est invalide")
                .When(x => x.Data.Frequency.HasValue);

            // Validation de l'intervalle si fourni
            RuleFor(x => x.Data.Interval)
                .GreaterThanOrEqualTo(1)
                .WithMessage("L'intervalle doit etre superieur ou egal a 1")
                .When(x => x.Data.Interval.HasValue);

            // Validation du jour du mois si fourni
            RuleFor(x => x.Data.DayOfMonth)
                .InclusiveBetween(1, 31)
                .WithMessage("Le jour du mois doit etre entre 1 et 31")
                .When(x => x.Data.DayOfMonth.HasValue);

            // Validation du jour de la semaine si fourni
            RuleFor(x => x.Data.DayOfWeek)
                .InclusiveBetween(1, 7)
                .WithMessage("Le jour de la semaine doit etre entre 1 et 7")
                .When(x => x.Data.DayOfWeek.HasValue);

            // Validation de la date de fin si fournie
            RuleFor(x => x.Data.EndDate)
                .GreaterThan(x => x.Data.StartDate ?? DateTime.MinValue)
                .WithMessage("La date de fin doit etre posterieure a la date de debut")
                .When(x => x.Data.EndDate.HasValue && x.Data.StartDate.HasValue);

            // Validation du nom du tiers si fourni
            RuleFor(x => x.Data.ThirdPartyName)
                .MaximumLength(200)
                .WithMessage("Le nom du tiers ne peut pas depasser 200 caracteres")
                .When(x => x.Data.ThirdPartyName != null);
        });
    }
}
