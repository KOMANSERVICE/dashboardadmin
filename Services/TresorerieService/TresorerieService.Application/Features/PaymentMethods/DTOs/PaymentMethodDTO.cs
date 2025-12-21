using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.PaymentMethods.DTOs;

public record PaymentMethodDTO(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string Name,
    PaymentMethodType Type,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
