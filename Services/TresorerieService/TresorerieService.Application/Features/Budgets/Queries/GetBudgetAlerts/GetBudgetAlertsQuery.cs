using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetAlerts;

/// <summary>
/// Query pour recuperer les alertes budgetaires d'une boutique
/// Retourne les budgets ayant atteint le seuil d'alerte ou depasses
/// </summary>
public record GetBudgetAlertsQuery(
    string ApplicationId,
    string BoutiqueId
) : IQuery<GetBudgetAlertsResponse>;
