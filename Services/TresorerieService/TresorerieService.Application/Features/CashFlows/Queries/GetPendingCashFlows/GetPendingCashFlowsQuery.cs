namespace TresorerieService.Application.Features.CashFlows.Queries.GetPendingCashFlows;

/// <summary>
/// Query pour recuperer la liste des flux en attente de validation (Status = PENDING)
/// </summary>
public record GetPendingCashFlowsQuery(
    // Identifiants obligatoires
    string ApplicationId,
    string BoutiqueId,

    // Filtres optionnels
    CashFlowType? Type = null,
    Guid? AccountId = null
) : IQuery<GetPendingCashFlowsResponse>;
