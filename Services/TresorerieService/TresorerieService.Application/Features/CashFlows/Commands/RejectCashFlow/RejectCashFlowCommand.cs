using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.RejectCashFlow;

public record RejectCashFlowCommand(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string RejectionReason,
    string RejectedBy,
    string UserRole
) : ICommand<RejectCashFlowResult>;

public record RejectCashFlowResult(CashFlowDTO CashFlow);
