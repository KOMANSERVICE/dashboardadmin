using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.CreateRecurringCashFlow;

public record CreateRecurringCashFlowCommand(
    string ApplicationId,
    string BoutiqueId,
    CashFlowType Type,
    string CategoryId,
    string Label,
    string? Description,
    decimal Amount,
    Guid AccountId,
    string PaymentMethod,
    string? ThirdPartyName,
    RecurringFrequency Frequency,
    int Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime StartDate,
    DateTime? EndDate,
    bool AutoValidate,
    string CreatedBy
) : ICommand<CreateRecurringCashFlowResult>;

public record CreateRecurringCashFlowResult(
    RecurringCashFlowDTO RecurringCashFlow
);
