using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Commands.UpdateBudget;

public record UpdateBudgetCommand(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? AllocatedAmount,
    List<Guid>? CategoryIds,
    int? AlertThreshold
) : ICommand<UpdateBudgetResult>;

public record UpdateBudgetResult(
    BudgetDTO Budget,
    string? Warning
);
