using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Budgets.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetAlerts;

/// <summary>
/// Handler pour recuperer les alertes budgetaires
/// Retourne les budgets actifs ayant atteint le seuil d'alerte ou depasses
/// Tries par criticite: depasses en premier, puis par taux de consommation decroissant
/// </summary>
public class GetBudgetAlertsHandler(IGenericRepository<Budget> budgetRepository)
    : IQueryHandler<GetBudgetAlertsQuery, GetBudgetAlertsResponse>
{
    private const string AlertLevelExceeded = "EXCEEDED";
    private const string AlertLevelApproaching = "APPROACHING";

    public async Task<GetBudgetAlertsResponse> Handle(
        GetBudgetAlertsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer les budgets actifs de la boutique
        var budgets = await budgetRepository.GetByConditionAsync(
            b => b.ApplicationId == query.ApplicationId
                 && b.BoutiqueId == query.BoutiqueId
                 && b.IsActive,
            cancellationToken);

        // Filtrer et mapper les budgets en alerte
        var alertDtos = budgets
            .Select(b =>
            {
                // Calcul du taux de consommation (eviter division par zero)
                var consumptionRate = b.AllocatedAmount > 0
                    ? Math.Round((b.SpentAmount / b.AllocatedAmount) * 100, 2)
                    : 0m;

                // Determiner le niveau d'alerte
                string? alertLevel = null;
                if (b.IsExceeded)
                {
                    alertLevel = AlertLevelExceeded;
                }
                else if (consumptionRate >= b.AlertThreshold)
                {
                    alertLevel = AlertLevelApproaching;
                }

                return new
                {
                    Budget = b,
                    ConsumptionRate = consumptionRate,
                    AlertLevel = alertLevel
                };
            })
            .Where(x => x.AlertLevel != null) // Garder seulement ceux en alerte
            .Select(x => new BudgetAlertDto(
                BudgetId: x.Budget.Id,
                BudgetName: x.Budget.Name,
                Type: x.Budget.Type,
                AllocatedAmount: x.Budget.AllocatedAmount,
                SpentAmount: x.Budget.SpentAmount,
                RemainingAmount: x.Budget.RemainingAmount,
                AlertThreshold: x.Budget.AlertThreshold,
                IsExceeded: x.Budget.IsExceeded,
                ConsumptionRate: x.ConsumptionRate,
                AlertLevel: x.AlertLevel!,
                Currency: x.Budget.Currency,
                StartDate: x.Budget.StartDate,
                EndDate: x.Budget.EndDate
            ))
            // Trier par criticite: depasses en premier, puis par taux de consommation decroissant
            .OrderByDescending(a => a.IsExceeded)
            .ThenByDescending(a => a.ConsumptionRate)
            .ToList();

        // Compter les budgets depasses et proches de l'alerte
        var exceededCount = alertDtos.Count(a => a.IsExceeded);
        var approachingCount = alertDtos.Count(a => !a.IsExceeded);

        return new GetBudgetAlertsResponse(
            Alerts: alertDtos,
            TotalCount: alertDtos.Count,
            ExceededCount: exceededCount,
            ApproachingCount: approachingCount
        );
    }
}
