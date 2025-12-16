using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.SubmitCashFlow;

public record SubmitCashFlowCommand(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string SubmittedBy
) : ICommand<SubmitCashFlowResult>;

public record SubmitCashFlowResult(
    CashFlowDTO CashFlow,
    string? BudgetWarning
);
