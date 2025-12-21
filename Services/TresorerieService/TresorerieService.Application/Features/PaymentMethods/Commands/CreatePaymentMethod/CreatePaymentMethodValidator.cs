using FluentValidation;

namespace TresorerieService.Application.Features.PaymentMethods.Commands.CreatePaymentMethod;

public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom de la méthode de paiement est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom de la méthode de paiement ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Le type de méthode de paiement doit être valide (CASH, CARD, TRANSFER, CHECK ou MOBILE)");
    }
}
