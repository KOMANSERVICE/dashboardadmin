namespace MagasinService.Domain.ValueObjects;

public record StockSlipItemId
{
    public Guid Value { get; }
    private StockSlipItemId(Guid value) => Value = value;
    public static StockSlipItemId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("StockSlipItemId cannot be empty.");
        }
        return new StockSlipItemId(value);
    }
}