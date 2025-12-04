using FluentValidation;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public class CreateStockSlipValidator : AbstractValidator<CreateStockSlipCommand>
{
    public CreateStockSlipValidator()
    {
        RuleFor(x => x.Request.Reference)
            .NotEmpty().WithMessage("La référence est obligatoire")
            .MaximumLength(100).WithMessage("La référence ne peut pas dépasser 100 caractères");

        RuleFor(x => x.Request.BoutiqueId)
            .NotEmpty().WithMessage("L'identifiant de la boutique est obligatoire");

        RuleFor(x => x.Request.SlipType)
            .IsInEnum().WithMessage("Le type de bordereau n'est pas valide");

        RuleFor(x => x.Request.SourceLocationId)
            .NotEmpty().WithMessage("Le magasin source est obligatoire");

        RuleFor(x => x.Request.DestinationLocationId)
            .NotEmpty().WithMessage("Le magasin destination est obligatoire")
            .When(x => x.Request.SlipType == StockSlipType.Transfer);

        RuleFor(x => x.Request.Items)
            .NotEmpty().WithMessage("Le bordereau doit contenir au moins un article");

        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("L'identifiant du produit est obligatoire");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");

            item.RuleFor(x => x.Note)
                .MaximumLength(250).WithMessage("La note ne peut pas dépasser 250 caractères");
        });
    }
}