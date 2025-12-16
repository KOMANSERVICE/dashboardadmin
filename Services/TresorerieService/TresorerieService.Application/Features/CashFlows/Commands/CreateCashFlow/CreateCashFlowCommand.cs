using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlow;

public record CreateCashFlowCommand(
    string ApplicationId,
    string BoutiqueId,
    CashFlowType Type,
    string CategoryId,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    DateTime Date,
    string? ThirdPartyName,
    string? AttachmentUrl,
    string CreatedBy
) : ICommand<CreateCashFlowResult>;

public record CreateCashFlowResult(
    CashFlowDTO CashFlow,
    string? BudgetWarning
);
