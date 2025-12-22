using TresorerieService.Application.Features.Budgets.DTOs;

namespace TresorerieService.Application.Features.Budgets.Commands.CreateBudget;

public record CreateBudgetCommand(
    string ApplicationId,
    string BoutiqueId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedAmount,
    List<Guid> CategoryIds,
    int AlertThreshold,
    BudgetType Type
) : ICommand<CreateBudgetResult>;

public record CreateBudgetResult(BudgetDTO Budget);
