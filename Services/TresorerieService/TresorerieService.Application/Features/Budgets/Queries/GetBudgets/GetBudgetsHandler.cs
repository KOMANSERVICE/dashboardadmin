using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Budgets.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgets;

public class GetBudgetsHandler(IGenericRepository<Budget> budgetRepository)
    : IQueryHandler<GetBudgetsQuery, GetBudgetsResponse>
{
    public async Task<GetBudgetsResponse> Handle(
        GetBudgetsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer les budgets selon les filtres
        var budgets = await budgetRepository.GetByConditionAsync(
            b => b.ApplicationId == query.ApplicationId
                 && b.BoutiqueId == query.BoutiqueId
                 && (!query.IsActive.HasValue || b.IsActive == query.IsActive.Value)
                 && (!query.StartDate.HasValue || b.EndDate >= query.StartDate.Value)
                 && (!query.EndDate.HasValue || b.StartDate <= query.EndDate.Value),
            cancellationToken);

        // Mapper vers les DTOs avec calculs
        var budgetDtos = budgets
            .Select(b =>
            {
                // Calcul du pourcentage utilise (eviter division par zero)
                var percentUsed = b.AllocatedAmount > 0
                    ? Math.Round((b.SpentAmount / b.AllocatedAmount) * 100, 2)
                    : 0m;

                // Determiner si proche du seuil d'alerte (mais pas encore depasse)
                var isNearAlert = !b.IsExceeded && percentUsed >= b.AlertThreshold;

                return new BudgetListDto(
                    Id: b.Id,
                    Name: b.Name,
                    StartDate: b.StartDate,
                    EndDate: b.EndDate,
                    AllocatedAmount: b.AllocatedAmount,
                    SpentAmount: b.SpentAmount,
                    RemainingAmount: b.RemainingAmount,
                    PercentUsed: percentUsed,
                    Currency: b.Currency,
                    Type: b.Type,
                    AlertThreshold: b.AlertThreshold,
                    IsExceeded: b.IsExceeded,
                    IsNearAlert: isNearAlert,
                    IsActive: b.IsActive,
                    CreatedAt: b.CreatedAt
                );
            })
            .OrderByDescending(b => b.IsExceeded)
            .ThenByDescending(b => b.IsNearAlert)
            .ThenByDescending(b => b.PercentUsed)
            .ToList();

        // Compter les budgets depasses et proches de l'alerte
        var exceededCount = budgetDtos.Count(b => b.IsExceeded);
        var nearAlertCount = budgetDtos.Count(b => b.IsNearAlert);

        return new GetBudgetsResponse(
            Budgets: budgetDtos,
            TotalCount: budgetDtos.Count,
            ExceededCount: exceededCount,
            NearAlertCount: nearAlertCount
        );
    }
}
