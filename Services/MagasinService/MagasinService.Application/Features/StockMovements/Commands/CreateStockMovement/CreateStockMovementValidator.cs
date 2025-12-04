using FluentValidation;
using MagasinService.Application.Features.StockMovements.DTOs;
using MagasinService.Domain.Enums;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.Request.Quantity)
            .GreaterThan(0)
            .WithMessage("La quantité doit être supérieure à 0");

        RuleFor(x => x.Request.Reference)
            .NotEmpty()
            .WithMessage("La référence est obligatoire")
            .MaximumLength(50)
            .WithMessage("La référence ne peut pas dépasser 50 caractères");

        RuleFor(x => x.Request.ProductId)
            .NotEmpty()
            .WithMessage("L'identifiant du produit est obligatoire");

        RuleFor(x => x.Request.SourceLocationId)
            .NotEmpty()
            .WithMessage("L'identifiant du magasin source est obligatoire");

        RuleFor(x => x.Request.DestinationLocationId)
            .NotEmpty()
            .WithMessage("L'identifiant du magasin destination est obligatoire")
            .NotEqual(x => x.Request.SourceLocationId)
            .WithMessage("Le magasin source et destination ne peuvent pas être identiques");

        RuleFor(x => x.Request.MovementType)
            .IsInEnum()
            .WithMessage("Le type de mouvement est invalide")
            .Equal(StockMovementType.Transfer)
            .When(x => x.Request.SourceLocationId != Guid.Empty && x.Request.DestinationLocationId != Guid.Empty)
            .WithMessage("Le type de mouvement doit être 'Transfer' pour un mouvement inter-magasin");
    }
}