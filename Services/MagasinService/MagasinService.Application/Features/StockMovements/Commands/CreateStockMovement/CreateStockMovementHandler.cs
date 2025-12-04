using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementHandler : ICommandHandler<CreateStockMovementCommand, CreateStockMovementResponse>
{
    private readonly IMagasinServiceDbContext _context;

    public CreateStockMovementHandler(IMagasinServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CreateStockMovementResponse> Handle(
        CreateStockMovementCommand request,
        CancellationToken cancellationToken)
    {
        // Vérifier que les magasins source et destination existent
        var sourceLocation = await _context.StockLocations
            .FirstOrDefaultAsync(x => x.Id == StockLocationId.Of(request.SourceLocationId), cancellationToken);

        if (sourceLocation == null)
        {
            return new CreateStockMovementResponse
            {
                Success = false,
                Message = "Magasin source introuvable"
            };
        }

        var destinationLocation = await _context.StockLocations
            .FirstOrDefaultAsync(x => x.Id == StockLocationId.Of(request.DestinationLocationId), cancellationToken);

        if (destinationLocation == null)
        {
            return new CreateStockMovementResponse
            {
                Success = false,
                Message = "Magasin destination introuvable"
            };
        }

        // Vérifier que les deux magasins appartiennent à la même boutique
        if (sourceLocation.BoutiqueId != request.BoutiqueId || destinationLocation.BoutiqueId != request.BoutiqueId)
        {
            return new CreateStockMovementResponse
            {
                Success = false,
                Message = "Les magasins doivent appartenir à la même boutique"
            };
        }

        // Créer le mouvement de stock
        var stockMovement = new StockMovement
        {
            Id = StockMovementId.Of(Guid.NewGuid()),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Date = DateTime.UtcNow,
            Reference = string.IsNullOrEmpty(request.Reference)
                ? $"MOV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}"
                : request.Reference,
            MovementType = request.MovementType,
            SourceLocationId = StockLocationId.Of(request.SourceLocationId),
            DestinationLocationId = StockLocationId.Of(request.DestinationLocationId)
        };

        _context.StockMovements.Add(stockMovement);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateStockMovementResponse
        {
            Id = stockMovement.Id.Value,
            Reference = stockMovement.Reference,
            Success = true,
            Message = "Mouvement de stock créé avec succès"
        };
    }
}