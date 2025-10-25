using BackendAdmin.Domain.Exceptions;

namespace BackendAdmin.Domain.ValueObjects;

public record AppAdminId
{
    public Guid Value { get; }
    private AppAdminId(Guid value) => Value = value;
    public static AppAdminId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("AppAdminId cannot be empty.");
        }

        return new AppAdminId(value);
    }
}
