using FluentValidation;

namespace TresorerieService.Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("L'identifiant du compte est obligatoire");

        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Data)
            .NotNull()
            .WithMessage("Les données de mise à jour sont obligatoires");

        RuleFor(x => x.Data.Name)
            .NotEmpty()
            .WithMessage("Le nom du compte est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom du compte ne peut pas dépasser 200 caractères")
            .When(x => x.Data != null);

        RuleFor(x => x.Data.AlertThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le seuil d'alerte ne peut pas être négatif")
            .When(x => x.Data?.AlertThreshold.HasValue == true);

        RuleFor(x => x.Data.OverdraftLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le découvert autorisé ne peut pas être négatif")
            .When(x => x.Data?.OverdraftLimit.HasValue == true);

        RuleFor(x => x.Data.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le solde initial ne peut pas être négatif")
            .When(x => x.Data?.InitialBalance.HasValue == true);
    }
}
