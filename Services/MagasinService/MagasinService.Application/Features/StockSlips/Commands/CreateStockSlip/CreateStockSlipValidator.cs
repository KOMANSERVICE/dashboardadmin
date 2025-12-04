using FluentValidation;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public class CreateStockSlipValidator : AbstractValidator<CreateStockSlipCommand>
{
    public CreateStockSlipValidator()
    {
        RuleFor(x => x.BoutiqueId)
            .NotEmpty().WithMessage("La boutique est requise");

        RuleFor(x => x.SourceLocationId)
            .NotEmpty().WithMessage("Le magasin source est requis");

        RuleFor(x => x.DestinationLocationId)
            .NotEmpty().WithMessage("Le magasin destination est requis");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La note ne peut pas dépasser 500 caractères");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Au moins un article est requis")
            .Must(items => items.Count > 0).WithMessage("Au moins un article est requis");

        RuleForEach(x => x.Items).SetValidator(new StockSlipItemValidator());
    }
}

public class StockSlipItemValidator : AbstractValidator<StockSlipItemDto>
{
    public StockSlipItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Le produit est requis");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix unitaire ne peut pas être négatif");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La note ne peut pas dépasser 500 caractères");
    }
}