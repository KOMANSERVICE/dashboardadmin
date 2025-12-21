namespace TresorerieService.Application.Features.CashFlows.Queries.GetUnreconciledCashFlows;

/// <summary>
/// Réponse de la query GetUnreconciledCashFlows
/// </summary>
public record GetUnreconciledCashFlowsResponse(
    IReadOnlyList<UnreconciledCashFlowDto> CashFlows,
    int UnreconciledCount,
    decimal TotalUnreconciledAmount
);
