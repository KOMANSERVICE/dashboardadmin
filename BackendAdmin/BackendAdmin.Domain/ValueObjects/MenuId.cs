using BackendAdmin.Domain.Exceptions;

namespace BackendAdmin.Domain.ValueObjects;

public record MenuId
{
    public Guid Value { get; }
    private MenuId(Guid value) => Value = value;
    public static MenuId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("MenuId cannot be empty.");
        }

        return new MenuId(value);
    }
}
