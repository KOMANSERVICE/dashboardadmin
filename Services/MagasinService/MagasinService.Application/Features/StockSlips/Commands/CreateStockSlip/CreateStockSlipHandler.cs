using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public class CreateStockSlipHandler : ICommandHandler<CreateStockSlipCommand, CreateStockSlipResponse>
{
    private readonly IMagasinServiceDbContext _context;

    public CreateStockSlipHandler(IMagasinServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CreateStockSlipResponse> Handle(
        CreateStockSlipCommand request,
        CancellationToken cancellationToken)
    {
        // Vérifier que les magasins source et destination existent
        var sourceLocation = await _context.StockLocations
            .FirstOrDefaultAsync(x => x.Id == StockLocationId.Of(request.SourceLocationId), cancellationToken);

        if (sourceLocation == null)
        {
            return new CreateStockSlipResponse
            {
                Success = false,
                Message = "Magasin source introuvable"
            };
        }

        var destinationLocation = await _context.StockLocations
            .FirstOrDefaultAsync(x => x.Id == StockLocationId.Of(request.DestinationLocationId), cancellationToken);

        if (destinationLocation == null)
        {
            return new CreateStockSlipResponse
            {
                Success = false,
                Message = "Magasin destination introuvable"
            };
        }

        // Vérifier que les deux magasins appartiennent à la même boutique
        if (sourceLocation.BoutiqueId != request.BoutiqueId || destinationLocation.BoutiqueId != request.BoutiqueId)
        {
            return new CreateStockSlipResponse
            {
                Success = false,
                Message = "Les magasins doivent appartenir à la même boutique"
            };
        }

        // Créer le bordereau
        var stockSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = GenerateReference(request.IsInbound),
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            IsInbound = request.IsInbound,
            StockSlipItems = new List<StockSlipItem>()
        };

        // Créer les mouvements de stock et les items du bordereau
        foreach (var item in request.Items)
        {
            // Créer le mouvement de stock
            var stockMovement = new StockMovement
            {
                Id = StockMovementId.Of(Guid.NewGuid()),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Date = stockSlip.Date,
                Reference = $"{stockSlip.Reference}-{request.Items.IndexOf(item) + 1:D3}",
                MovementType = DetermineMovementType(request.IsInbound, request.SourceLocationId, request.DestinationLocationId),
                SourceLocationId = StockLocationId.Of(request.SourceLocationId),
                DestinationLocationId = StockLocationId.Of(request.DestinationLocationId)
            };

            _context.StockMovements.Add(stockMovement);

            // Créer l'item du bordereau
            var stockSlipItem = new StockSlipItem
            {
                Id = StockSlipItemId.Of(Guid.NewGuid()),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Note = item.Note,
                StockSlipId = stockSlip.Id,
                StockMovementId = stockMovement.Id
            };

            stockSlip.StockSlipItems.Add(stockSlipItem);
        }

        _context.StockSlips.Add(stockSlip);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateStockSlipResponse
        {
            Id = stockSlip.Id.Value,
            Reference = stockSlip.Reference,
            ItemsCount = stockSlip.StockSlipItems.Count,
            Success = true,
            Message = "Bordereau de mouvement créé avec succès"
        };
    }

    private string GenerateReference(bool isInbound)
    {
        var prefix = isInbound ? "BE" : "BS"; // BE = Bordereau d'Entrée, BS = Bordereau de Sortie
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    private StockMovementType DetermineMovementType(bool isInbound, Guid sourceId, Guid destinationId)
    {
        if (sourceId == destinationId)
        {
            return isInbound ? StockMovementType.In : StockMovementType.Out;
        }
        return StockMovementType.Transfer;
    }
}