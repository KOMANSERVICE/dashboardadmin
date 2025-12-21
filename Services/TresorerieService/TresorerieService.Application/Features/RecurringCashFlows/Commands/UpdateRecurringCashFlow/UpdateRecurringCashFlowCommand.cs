using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.UpdateRecurringCashFlow;

/// <summary>
/// Commande pour modifier un flux de tresorerie recurrent.
/// Champs NON modifiables: Id, CreatedBy, CreatedAt
/// Si la frequence est modifiee (Frequency, Interval, DayOfMonth, DayOfWeek), NextOccurrence est recalculee.
/// Les flux deja generes ne sont pas affectes.
/// </summary>
public record UpdateRecurringCashFlowCommand(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string UserId,
    UpdateRecurringCashFlowDto Data
) : ICommand<UpdateRecurringCashFlowResult>;

public record UpdateRecurringCashFlowResult(
    RecurringCashFlowDTO RecurringCashFlow
);
