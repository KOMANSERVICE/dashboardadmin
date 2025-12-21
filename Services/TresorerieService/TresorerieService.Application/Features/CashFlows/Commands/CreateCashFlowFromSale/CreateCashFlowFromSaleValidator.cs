namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromSale;

public class CreateCashFlowFromSaleValidator : AbstractValidator<CreateCashFlowFromSaleCommand>
{
    public CreateCashFlowFromSaleValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("L'identifiant de la vente est obligatoire");

        RuleFor(x => x.SaleReference)
            .NotEmpty()
            .WithMessage("La reference de la vente est obligatoire")
            .MaximumLength(100)
            .WithMessage("La reference de la vente ne peut pas depasser 100 caracteres");

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

        RuleFor(x => x.SaleDate)
            .NotEmpty()
            .WithMessage("La date de la vente est obligatoire");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .WithMessage("Le nom du client ne peut pas depasser 200 caracteres")
            .When(x => x.CustomerName != null);

        RuleFor(x => x.CustomerId)
            .MaximumLength(100)
            .WithMessage("L'identifiant du client ne peut pas depasser 100 caracteres")
            .When(x => x.CustomerId != null);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("L'identifiant de la categorie est obligatoire");
    }
}
