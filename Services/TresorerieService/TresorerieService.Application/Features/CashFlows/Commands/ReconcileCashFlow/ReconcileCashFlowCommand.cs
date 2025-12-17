using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlow;

public record ReconcileCashFlowCommand(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string ReconciledBy,
    string UserRole,
    string? BankStatementReference
) : ICommand<ReconcileCashFlowResult>;

public record ReconcileCashFlowResult(
    CashFlowDetailDto CashFlow
);
