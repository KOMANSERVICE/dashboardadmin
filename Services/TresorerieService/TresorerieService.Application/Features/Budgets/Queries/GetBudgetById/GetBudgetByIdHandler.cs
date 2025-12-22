using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Budgets.DTOs;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Budgets.Queries.GetBudgetById;

public class GetBudgetByIdHandler(
    IGenericRepository<Budget> budgetRepository,
    IGenericRepository<BudgetCategory> budgetCategoryRepository,
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository)
    : IQueryHandler<GetBudgetByIdQuery, GetBudgetByIdResponse>
{
    public async Task<GetBudgetByIdResponse> Handle(
        GetBudgetByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // 1. Recuperer le budget par Id et verifier qu'il appartient a l'application/boutique
        var budget = await budgetRepository.GetByIdAsync(query.BudgetId, cancellationToken);

        if (budget == null ||
            budget.ApplicationId != query.ApplicationId ||
            budget.BoutiqueId != query.BoutiqueId)
        {
            throw new NotFoundException("Budget", query.BudgetId);
        }

        // 2. Recuperer les categories associees au budget
        var budgetCategories = await budgetCategoryRepository.GetByConditionAsync(
            bc => bc.BudgetId == query.BudgetId,
            cancellationToken);

        var categoryIds = budgetCategories.Select(bc => bc.CategoryId).ToList();

        // 3. Recuperer les categories pour les noms et icones
        var categories = await categoryRepository.GetByConditionAsync(
            c => categoryIds.Contains(c.Id),
            cancellationToken);

        var categoryDict = categories.ToDictionary(c => c.Id);

        // 4. Recuperer les CashFlows EXPENSE lies aux categories du budget
        // Filtrer par: type EXPENSE, categories du budget, periode du budget, status APPROVED
        var categoryIdStrings = categoryIds.Select(id => id.ToString()).ToList();

        var expenses = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId
                  && cf.BoutiqueId == query.BoutiqueId
                  && cf.Type == CashFlowType.EXPENSE
                  && categoryIdStrings.Contains(cf.CategoryId)
                  && cf.Date >= budget.StartDate
                  && cf.Date <= budget.EndDate
                  && cf.Status == CashFlowStatus.APPROVED,
            cancellationToken);

        // 5. Calculer les metriques
        var spentAmount = expenses.Sum(e => e.Amount);
        var percentUsed = budget.AllocatedAmount > 0
            ? Math.Round((spentAmount / budget.AllocatedAmount) * 100, 2)
            : 0m;

        // 6. Mapper les depenses vers DTOs
        var expenseDtos = expenses
            .OrderByDescending(e => e.Date)
            .Select(e =>
            {
                var categoryName = "Inconnu";
                if (Guid.TryParse(e.CategoryId, out var catId) && categoryDict.TryGetValue(catId, out var cat))
                {
                    categoryName = cat.Name;
                }

                return new BudgetExpenseDto(
                    Id: e.Id,
                    Label: e.Label,
                    Description: e.Description,
                    Amount: e.Amount,
                    Currency: e.Currency,
                    Date: e.Date,
                    CategoryId: e.CategoryId,
                    CategoryName: categoryName,
                    Status: e.Status,
                    ThirdPartyName: e.ThirdPartyName
                );
            })
            .ToList();

        // 7. Calculer la repartition par categorie
        var categoryBreakdown = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                var amount = g.Sum(e => e.Amount);
                var percentage = spentAmount > 0
                    ? Math.Round((amount / spentAmount) * 100, 2)
                    : 0m;

                var categoryName = "Inconnu";
                string? categoryIcon = null;
                var catId = Guid.Empty;

                if (Guid.TryParse(g.Key, out catId) && categoryDict.TryGetValue(catId, out var cat))
                {
                    categoryName = cat.Name;
                    categoryIcon = cat.Icon;
                }

                return new CategoryBreakdownDto(
                    CategoryId: catId,
                    CategoryName: categoryName,
                    CategoryIcon: categoryIcon,
                    Amount: amount,
                    Percentage: percentage,
                    TransactionCount: g.Count()
                );
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // 8. Calculer l'evolution dans le temps (par mois)
        var monthlyData = expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .ToList();

        var timeEvolution = new List<TimeSeriesDto>();
        decimal cumulativeAmount = 0;

        foreach (var month in monthlyData)
        {
            var monthAmount = month.Sum(e => e.Amount);
            cumulativeAmount += monthAmount;

            var monthLabel = new DateTime(month.Key.Year, month.Key.Month, 1)
                .ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));

            timeEvolution.Add(new TimeSeriesDto(
                Year: month.Key.Year,
                Month: month.Key.Month,
                MonthLabel: monthLabel,
                Amount: monthAmount,
                CumulativeAmount: cumulativeAmount
            ));
        }

        // 9. Construire le DTO de reponse
        var budgetDetail = new BudgetDetailDto(
            Id: budget.Id,
            ApplicationId: budget.ApplicationId,
            BoutiqueId: budget.BoutiqueId,
            Name: budget.Name,
            StartDate: budget.StartDate,
            EndDate: budget.EndDate,
            AllocatedAmount: budget.AllocatedAmount,
            SpentAmount: spentAmount,
            RemainingAmount: budget.AllocatedAmount - spentAmount,
            PercentUsed: percentUsed,
            Currency: budget.Currency,
            Type: budget.Type,
            AlertThreshold: budget.AlertThreshold,
            IsExceeded: spentAmount > budget.AllocatedAmount,
            IsActive: budget.IsActive,
            CreatedAt: budget.CreatedAt,
            UpdatedAt: budget.UpdatedAt,
            Expenses: expenseDtos,
            CategoryBreakdown: categoryBreakdown,
            TimeEvolution: timeEvolution
        );

        return new GetBudgetByIdResponse(budgetDetail);
    }
}
