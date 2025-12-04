namespace MagasinService.Application.Features.StockSlips.DTOs;

public record CreateStockSlipRequest
{
    public string Reference { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public Guid BoutiqueId { get; init; }
    public StockSlipType SlipType { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid? DestinationLocationId { get; init; }
    public List<CreateStockSlipItemRequest> Items { get; init; } = new();
}

public record CreateStockSlipItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Note { get; init; } = string.Empty;
}