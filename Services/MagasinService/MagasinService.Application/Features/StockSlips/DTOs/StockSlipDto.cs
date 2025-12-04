namespace MagasinService.Application.Features.StockSlips.DTOs;

public record StockSlipDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string Note { get; init; } = string.Empty;
    public Guid BoutiqueId { get; init; }
    public StockSlipType SlipType { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid? DestinationLocationId { get; init; }
    public StockLocationDTO? SourceLocation { get; init; }
    public StockLocationDTO? DestinationLocation { get; init; }
    public List<StockSlipItemDto> Items { get; init; } = new();
}