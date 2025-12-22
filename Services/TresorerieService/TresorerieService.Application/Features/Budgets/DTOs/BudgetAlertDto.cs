using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Budgets.DTOs;

/// <summary>
/// DTO pour l'affichage d'une alerte budgetaire
/// Contient les informations essentielles pour les alertes
/// </summary>
public record BudgetAlertDto(
    Guid BudgetId,
    string BudgetName,
    BudgetType Type,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    int AlertThreshold,
    bool IsExceeded,
    decimal ConsumptionRate,
    string AlertLevel,
    string Currency,
    DateTime StartDate,
    DateTime EndDate
);
