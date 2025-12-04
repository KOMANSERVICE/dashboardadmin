namespace MagasinService.Domain.ValueObjects;

public record StockMovementId
{
    public Guid Value { get; }
    private StockMovementId(Guid value) => Value = value;
    public static StockMovementId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("StockMovementId cannot be empty.");
        }
        return new StockMovementId(value);
    }
}