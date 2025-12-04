using FluentValidation;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Le produit est requis");

        RuleFor(x => x.BoutiqueId)
            .NotEmpty().WithMessage("La boutique est requise");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");

        RuleFor(x => x.SourceLocationId)
            .NotEmpty().WithMessage("Le magasin source est requis")
            .NotEqual(x => x.DestinationLocationId).WithMessage("Le magasin source et destination doivent être différents");

        RuleFor(x => x.DestinationLocationId)
            .NotEmpty().WithMessage("Le magasin destination est requis");

        RuleFor(x => x.MovementType)
            .IsInEnum().WithMessage("Le type de mouvement est invalide");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("La référence ne peut pas dépasser 100 caractères");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("La note ne peut pas dépasser 500 caractères");
    }
}