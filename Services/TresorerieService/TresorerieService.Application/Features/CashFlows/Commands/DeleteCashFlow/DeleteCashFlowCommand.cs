namespace TresorerieService.Application.Features.CashFlows.Commands.DeleteCashFlow;

/// <summary>
/// Commande pour supprimer un flux de tresorerie en brouillon.
/// Seuls les flux avec Status = DRAFT peuvent etre supprimes.
/// L'utilisateur ne peut supprimer que ses propres flux.
/// La suppression est definitive.
/// </summary>
public record DeleteCashFlowCommand(
    Guid Id,
    string ApplicationId,
    string BoutiqueId
) : ICommand<DeleteCashFlowResult>;

public record DeleteCashFlowResult(bool Success);
