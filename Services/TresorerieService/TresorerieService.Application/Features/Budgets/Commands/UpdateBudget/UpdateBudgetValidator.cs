namespace TresorerieService.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("L'identifiant du budget est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Le nom du budget ne peut pas dépasser 200 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .WithMessage("La date de fin doit être postérieure à la date de début")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.AllocatedAmount)
            .GreaterThan(0)
            .WithMessage("Le montant alloué doit être supérieur à 0")
            .When(x => x.AllocatedAmount.HasValue);

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(0, 100)
            .WithMessage("Le seuil d'alerte doit être entre 0 et 100")
            .When(x => x.AlertThreshold.HasValue);
    }
}
