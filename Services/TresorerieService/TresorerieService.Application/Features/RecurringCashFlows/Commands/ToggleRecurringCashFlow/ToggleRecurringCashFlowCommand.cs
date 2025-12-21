namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.ToggleRecurringCashFlow;

/// <summary>
/// Commande pour activer/desactiver un flux de tresorerie recurrent.
/// Toggle IsActive: true -> false ou false -> true
/// </summary>
public record ToggleRecurringCashFlowCommand(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string UserId
) : ICommand<ToggleRecurringCashFlowResult>;

public record ToggleRecurringCashFlowResult(
    Guid Id,
    bool IsActive
);
