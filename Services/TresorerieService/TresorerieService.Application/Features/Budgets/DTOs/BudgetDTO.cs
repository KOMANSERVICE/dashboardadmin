using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Budgets.DTOs;

public record BudgetDTO(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    string Currency,
    BudgetType Type,
    int AlertThreshold,
    bool IsExceeded,
    bool IsActive,
    List<Guid> CategoryIds,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
