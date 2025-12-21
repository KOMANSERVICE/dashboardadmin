namespace TresorerieService.Application.Features.CashFlows.Queries.GetPendingCashFlows;

/// <summary>
/// Reponse contenant les flux en attente de validation avec compteur
/// </summary>
public record GetPendingCashFlowsResponse(
    IReadOnlyList<PendingCashFlowDto> CashFlows,
    int PendingCount
);
