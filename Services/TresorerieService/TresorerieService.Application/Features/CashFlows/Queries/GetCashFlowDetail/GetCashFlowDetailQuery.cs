namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlowDetail;

/// <summary>
/// Query pour recuperer le detail complet d'un flux de tresorerie
/// </summary>
public record GetCashFlowDetailQuery(
    Guid CashFlowId,
    string ApplicationId,
    string BoutiqueId,
    string UserId,
    bool IsManager
) : IQuery<GetCashFlowDetailResponse>;
