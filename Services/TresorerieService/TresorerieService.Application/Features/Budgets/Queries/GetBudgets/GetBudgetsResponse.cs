using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgets;

/// <summary>
/// Reponse pour la liste des budgets
/// </summary>
public record GetBudgetsResponse(
    IReadOnlyList<BudgetListDto> Budgets,
    int TotalCount,
    int ExceededCount,
    int NearAlertCount
);
