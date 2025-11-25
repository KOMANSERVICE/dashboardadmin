namespace MagasinService.Domain.ValueObjects;

public record StockLocationId
{
    public Guid Value { get; }
    private StockLocationId(Guid value) => Value = value;
    public static StockLocationId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("StockLocationId cannot be empty.");
        }
        return new StockLocationId(value);
    }
}
