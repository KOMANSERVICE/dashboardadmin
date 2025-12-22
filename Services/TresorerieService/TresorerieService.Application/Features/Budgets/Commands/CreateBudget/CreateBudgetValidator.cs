namespace TresorerieService.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du budget est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom du budget ne peut pas dépasser 200 caractères");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("La date de début est obligatoire");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("La date de fin est obligatoire")
            .GreaterThan(x => x.StartDate)
            .WithMessage("La date de fin doit être postérieure à la date de début");

        RuleFor(x => x.AllocatedAmount)
            .GreaterThan(0)
            .WithMessage("Le montant alloué doit être supérieur à 0");

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(0, 100)
            .WithMessage("Le seuil d'alerte doit être entre 0 et 100");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de budget doit être GLOBAL, CATEGORY ou PROJECT");
    }
}
