using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetUnreconciledCashFlows;

/// <summary>
/// Query pour récupérer les flux de trésorerie non réconciliés (APPROVED et IsReconciled = false)
/// </summary>
public record GetUnreconciledCashFlowsQuery(
    string ApplicationId,
    string BoutiqueId,
    Guid? AccountId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<GetUnreconciledCashFlowsResponse>;
