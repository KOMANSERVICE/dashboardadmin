using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlows;

/// <summary>
/// Reponse paginee pour la liste des flux de tresorerie
/// </summary>
public record GetCashFlowsResponse(
    IReadOnlyList<CashFlowListDto> CashFlows,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasPrevious,
    bool HasNext
);
