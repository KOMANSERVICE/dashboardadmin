using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetAlerts;

/// <summary>
/// Reponse pour les alertes budgetaires
/// Inclut un resume et la liste des alertes triees par criticite
/// </summary>
public record GetBudgetAlertsResponse(
    IReadOnlyList<BudgetAlertDto> Alerts,
    int TotalCount,
    int ExceededCount,
    int ApproachingCount
);
