using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.UpdateCashFlow;

/// <summary>
/// Commande pour modifier un flux de tresorerie en brouillon.
/// Seuls les flux avec Status = DRAFT peuvent etre modifies.
/// L'utilisateur ne peut modifier que ses propres flux.
/// </summary>
public record UpdateCashFlowCommand(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string UserId,
    UpdateCashFlowDto Data
) : ICommand<UpdateCashFlowResult>;

public record UpdateCashFlowResult(CashFlowDTO CashFlow);
