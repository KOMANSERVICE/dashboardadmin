using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using Mapster;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockMovements.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Exceptions;
using MagasinService.Domain.ValueObjects;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementHandler : ICommandHandler<CreateStockMovementCommand, StockMovementDto>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IStockLocationRepository _stockLocationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockMovementHandler(
        IStockMovementRepository stockMovementRepository,
        IStockLocationRepository stockLocationRepository,
        IUnitOfWork unitOfWork)
    {
        _stockMovementRepository = stockMovementRepository;
        _stockLocationRepository = stockLocationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StockMovementDto> Handle(CreateStockMovementCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Valider l'existence du magasin source
        var sourceLocation = await _stockLocationRepository.GetByIdAsync(request.SourceLocationId);
        if (sourceLocation == null)
        {
            throw new NotFoundException("StockLocation", request.SourceLocationId);
        }

        // Valider l'existence du magasin destination
        var destinationLocation = await _stockLocationRepository.GetByIdAsync(request.DestinationLocationId);
        if (destinationLocation == null)
        {
            throw new NotFoundException("StockLocation", request.DestinationLocationId);
        }

        // Vérifier que les deux magasins appartiennent à la même boutique
        if (sourceLocation.BoutiqueId != destinationLocation.BoutiqueId)
        {
            throw new DomainException("Les magasins source et destination doivent appartenir à la même boutique");
        }

        // Vérifier que source et destination sont différents
        if (request.SourceLocationId == request.DestinationLocationId)
        {
            throw new DomainException("Le magasin source et destination ne peuvent pas être identiques");
        }

        // Créer le mouvement de stock
        var stockMovement = new StockMovement
        {
            Id = StockMovementId.Of(Guid.NewGuid()),
            Quantity = request.Quantity,
            Date = DateTime.UtcNow,
            Reference = request.Reference,
            MovementType = request.MovementType,
            ProductId = request.ProductId,
            SourceLocationId = StockLocationId.Of(request.SourceLocationId),
            DestinationLocationId = StockLocationId.Of(request.DestinationLocationId)
        };

        await _stockMovementRepository.AddAsync(stockMovement);
        await _unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Récupérer le mouvement créé avec ses relations
        var createdMovement = await _stockMovementRepository.GetByIdAsync(stockMovement.Id.Value);

        return createdMovement!.Adapt<StockMovementDto>();
    }
}