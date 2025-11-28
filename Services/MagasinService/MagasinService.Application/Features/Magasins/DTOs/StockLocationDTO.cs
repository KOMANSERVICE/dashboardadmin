using MagasinService.Domain.Enums;

namespace MagasinService.Application.Features.Magasins.DTOs;

public record StockLocationDTO : StockLocationUpdateDTO
{
    public Guid Id { get; set; }
    public StockLocationType Type { get; set; } = StockLocationType.Sale;
}

public record StockLocationUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
