using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public record CreateStockSlipCommand : ICommand<CreateStockSlipResponse>
{
    public Guid BoutiqueId { get; init; } // Boutique provenant d'une autre application
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
    public string Note { get; init; } = string.Empty;
    public bool IsInbound { get; init; } // true = entrée, false = sortie
    public List<StockSlipItemDto> Items { get; init; } = new();
}

public record StockSlipItemDto
{
    public Guid ProductId { get; init; } // Produit provenant d'une autre application
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Note { get; init; } = string.Empty;
}

public record CreateStockSlipResponse
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public int ItemsCount { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}