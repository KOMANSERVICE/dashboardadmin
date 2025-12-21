namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromPurchase;

public class CreateCashFlowFromPurchaseValidator : AbstractValidator<CreateCashFlowFromPurchaseCommand>
{
    public CreateCashFlowFromPurchaseValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'application est obligatoire");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.PurchaseId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'achat est obligatoire");

        RuleFor(x => x.PurchaseReference)
            .NotEmpty()
            .WithMessage("La reference de l'achat est obligatoire")
            .MaximumLength(100)
            .WithMessage("La reference de l'achat ne peut pas depasser 100 caracteres");

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

        RuleFor(x => x.PurchaseDate)
            .NotEmpty()
            .WithMessage("La date de l'achat est obligatoire");

        RuleFor(x => x.SupplierName)
            .MaximumLength(200)
            .WithMessage("Le nom du fournisseur ne peut pas depasser 200 caracteres")
            .When(x => x.SupplierName != null);

        RuleFor(x => x.SupplierId)
            .MaximumLength(100)
            .WithMessage("L'identifiant du fournisseur ne peut pas depasser 100 caracteres")
            .When(x => x.SupplierId != null);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("L'identifiant de la categorie est obligatoire");
    }
}
