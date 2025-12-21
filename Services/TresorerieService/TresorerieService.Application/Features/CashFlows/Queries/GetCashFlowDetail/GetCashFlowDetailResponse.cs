using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlowDetail;

/// <summary>
/// Reponse contenant le detail complet d'un flux de tresorerie
/// </summary>
public record GetCashFlowDetailResponse(CashFlowDetailDto CashFlow);
