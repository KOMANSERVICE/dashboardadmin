using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlows;

/// <summary>
/// Commande pour reconcilier plusieurs flux de tresorerie en masse
/// </summary>
public record ReconcileCashFlowsCommand(
    IReadOnlyList<Guid> CashFlowIds,
    string ApplicationId,
    string BoutiqueId,
    string ReconciledBy,
    string UserRole,
    string? BankStatementReference
) : ICommand<ReconcileCashFlowsResult>;

/// <summary>
/// Resultat de la reconciliation en masse
/// </summary>
public record ReconcileCashFlowsResult(
    int ReconciledCount,
    IReadOnlyList<ReconciledCashFlowDto> ReconciledCashFlows
);

/// <summary>
/// DTO pour un flux reconcilie
/// </summary>
public record ReconciledCashFlowDto(
    Guid Id,
    string? Reference,
    string Label,
    decimal Amount,
    DateTime ReconciledAt,
    string? BankStatementReference
);
