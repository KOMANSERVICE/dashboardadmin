namespace TresorerieService.Application.Features.CashFlows.Commands.ReverseCashFlow;

/// <summary>
/// Commande pour creer une contre-passation d'un CashFlow.
/// La contre-passation cree un nouveau CashFlow avec le type inverse pour annuler l'effet du flux original.
/// </summary>
public record ReverseCashFlowCommand(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string Reason,
    string? SourceType = null,
    Guid? SourceId = null
) : ICommand<ReverseCashFlowResult>;

/// <summary>
/// Resultat de la contre-passation d'un CashFlow.
/// </summary>
public record ReverseCashFlowResult(
    Guid ReversalCashFlowId,
    Guid OriginalCashFlowId,
    bool Success
);
