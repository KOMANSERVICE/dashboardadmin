namespace MagasinService.Domain.ValueObjects;

public record StockSlipId
{
    public Guid Value { get; }
    private StockSlipId(Guid value) => Value = value;
    public static StockSlipId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("StockSlipId cannot be empty.");
        }
        return new StockSlipId(value);
    }
}