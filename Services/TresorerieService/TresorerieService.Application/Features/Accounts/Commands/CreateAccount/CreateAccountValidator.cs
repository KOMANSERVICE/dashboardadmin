using FluentValidation;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du compte est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom du compte ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La description ne peut pas dépasser 500 caractères")
            .When(x => x.Description != null);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de compte est obligatoire");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le solde initial ne peut pas être négatif");

        RuleFor(x => x.AlertThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le seuil d'alerte ne peut pas être négatif")
            .When(x => x.AlertThreshold.HasValue);

        RuleFor(x => x.OverdraftLimit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le découvert autorisé ne peut pas être négatif")
            .When(x => x.OverdraftLimit.HasValue);

        // Validation spécifique pour les comptes bancaires
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Le numéro de compte est obligatoire pour un compte bancaire")
            .When(x => x.Type == AccountType.BANK);

        RuleFor(x => x.BankName)
            .NotEmpty()
            .WithMessage("Le nom de la banque est obligatoire pour un compte bancaire")
            .When(x => x.Type == AccountType.BANK);

        RuleFor(x => x.AccountNumber)
            .MaximumLength(150)
            .WithMessage("Le numéro de compte ne peut pas dépasser 150 caractères")
            .When(x => !string.IsNullOrEmpty(x.AccountNumber));

        RuleFor(x => x.BankName)
            .MaximumLength(100)
            .WithMessage("Le nom de la banque ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        // Le découvert autorisé n'est applicable qu'aux comptes bancaires
        RuleFor(x => x.OverdraftLimit)
            .Null()
            .WithMessage("Le découvert autorisé n'est applicable qu'aux comptes bancaires")
            .When(x => x.Type != AccountType.BANK && x.OverdraftLimit.HasValue);
    }
}
