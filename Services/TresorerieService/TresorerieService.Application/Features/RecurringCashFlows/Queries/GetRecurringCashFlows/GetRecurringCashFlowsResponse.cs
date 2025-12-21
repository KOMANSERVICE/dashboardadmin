using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Queries.GetRecurringCashFlows;

/// <summary>
/// Reponse pour la liste des flux recurrents
/// </summary>
public record GetRecurringCashFlowsResponse(
    IReadOnlyList<RecurringCashFlowDTO> RecurringCashFlows,
    int TotalCount,
    decimal EstimatedMonthlyTotal
);
