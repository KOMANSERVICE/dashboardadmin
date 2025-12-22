using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetById;

/// <summary>
/// Reponse pour le detail d'un budget
/// </summary>
public record GetBudgetByIdResponse(
    BudgetDetailDto Budget
);
