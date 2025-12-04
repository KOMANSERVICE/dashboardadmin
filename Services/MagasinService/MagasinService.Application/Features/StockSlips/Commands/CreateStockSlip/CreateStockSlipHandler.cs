using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using Mapster;
using MagasinService.Domain.Exceptions;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public class CreateStockSlipHandler : ICommandHandler<CreateStockSlipCommand, StockSlipDto>
{
    private readonly IStockSlipRepository _stockSlipRepository;
    private readonly IStockLocationRepository _stockLocationRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockSlipHandler(
        IStockSlipRepository stockSlipRepository,
        IStockLocationRepository stockLocationRepository,
        IStockMovementRepository stockMovementRepository,
        IUnitOfWork unitOfWork)
    {
        _stockSlipRepository = stockSlipRepository;
        _stockLocationRepository = stockLocationRepository;
        _stockMovementRepository = stockMovementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StockSlipDto> Handle(CreateStockSlipCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Vérifier l'unicité de la référence
        var existingSlip = await _stockSlipRepository.GetByReferenceAsync(request.Reference, cancellationToken);
        if (existingSlip != null)
        {
            throw new DomainException($"Un bordereau avec la référence '{request.Reference}' existe déjà.");
        }

        // Vérifier l'existence du magasin source
        var sourceLocation = await _stockLocationRepository.GetByIdAsync(request.SourceLocationId);
        if (sourceLocation == null)
        {
            throw new NotFoundException("StockLocation", request.SourceLocationId);
        }

        // Vérifier l'existence du magasin destination si c'est un transfert
        if (request.SlipType == StockSlipType.Transfer && request.DestinationLocationId.HasValue)
        {
            var destinationLocation = await _stockLocationRepository.GetByIdAsync(request.DestinationLocationId.Value);
            if (destinationLocation == null)
            {
                throw new NotFoundException("StockLocation", request.DestinationLocationId.Value);
            }
        }

        // Créer le bordereau
        var stockSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = request.Reference,
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            SlipType = request.SlipType,
            SourceLocationId = StockLocationId.Of(request.SourceLocationId),
            DestinationLocationId = request.DestinationLocationId.HasValue
                ? StockLocationId.Of(request.DestinationLocationId.Value)
                : null
        };

        // Ajouter les items
        foreach (var itemRequest in request.Items)
        {
            var item = new StockSlipItem
            {
                Id = StockSlipItemId.Of(Guid.NewGuid()),
                StockSlipId = stockSlip.Id,
                ProductId = itemRequest.ProductId,
                Quantity = itemRequest.Quantity,
                Note = itemRequest.Note
            };

            stockSlip.StockSlipItems.Add(item);

            // Créer les mouvements de stock correspondants
            if (request.SlipType == StockSlipType.Transfer && request.DestinationLocationId.HasValue)
            {
                var movement = new StockMovement
                {
                    Id = StockMovementId.Of(Guid.NewGuid()),
                    Quantity = itemRequest.Quantity,
                    Date = DateTime.UtcNow,
                    Reference = request.Reference,
                    MovementType = StockMovementType.Transfer,
                    ProductId = itemRequest.ProductId,
                    SourceLocationId = StockLocationId.Of(request.SourceLocationId),
                    DestinationLocationId = StockLocationId.Of(request.DestinationLocationId.Value)
                };

                await _stockMovementRepository.AddAsync(movement);
            }
        }

        await _stockSlipRepository.AddAsync(stockSlip);
        await _unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Récupérer le bordereau avec ses relations
        var createdSlip = await _stockSlipRepository.GetSlipWithItemsAsync(stockSlip.Id, cancellationToken);

        return createdSlip!.Adapt<StockSlipDto>();
    }
}