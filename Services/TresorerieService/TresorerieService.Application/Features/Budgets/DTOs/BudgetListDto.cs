using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Budgets.DTOs;

/// <summary>
/// DTO pour l'affichage d'un budget dans une liste
/// Contient les calculs PercentUsed et IsNearAlert
/// </summary>
public record BudgetListDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal PercentUsed,
    string Currency,
    BudgetType Type,
    int AlertThreshold,
    bool IsExceeded,
    bool IsNearAlert,
    bool IsActive,
    DateTime CreatedAt
);
