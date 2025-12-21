using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.ApproveCashFlow;

public record ApproveCashFlowCommand(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string ValidatedBy,
    string UserRole
) : ICommand<ApproveCashFlowResult>;

public record ApproveCashFlowResult(
    CashFlowDTO CashFlow,
    decimal NewAccountBalance
);
