using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgets;

/// <summary>
/// Query pour recuperer la liste des budgets d'une boutique
/// Supporte le filtre par periode (startDate/endDate) et par statut actif
/// </summary>
public record GetBudgetsQuery(
    string ApplicationId,
    string BoutiqueId,
    bool? IsActive = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<GetBudgetsResponse>;
