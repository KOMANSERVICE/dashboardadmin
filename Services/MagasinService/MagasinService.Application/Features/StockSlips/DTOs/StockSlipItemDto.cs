namespace MagasinService.Application.Features.StockSlips.DTOs;

public record StockSlipItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Note { get; init; } = string.Empty;
}