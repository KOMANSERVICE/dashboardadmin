using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowStatement;

/// <summary>
/// Handler pour recuperer l'etat des flux de tresorerie
/// US-035: Etat des flux de tresorerie
/// </summary>
public class GetCashFlowStatementHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository)
    : IQueryHandler<GetCashFlowStatementQuery, GetCashFlowStatementResponse>
{
    public async Task<GetCashFlowStatementResponse> Handle(
        GetCashFlowStatementQuery query,
        CancellationToken cancellationToken = default)
    {
        // Normaliser les dates (debut de journee pour startDate, fin de journee pour endDate)
        var startDate = query.StartDate.Date;
        var endDate = query.EndDate.Date.AddDays(1).AddTicks(-1);

        // 1. Recuperer les categories de l'application
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == query.ApplicationId && c.IsActive,
            cancellationToken);

        var categoriesDict = categories.ToDictionary(c => c.Id.ToString(), c => c);

        // 2. Recuperer les flux APPROVED de la periode courante
        // Seuls les flux APPROVED sont comptabilises (pas DRAFT, PENDING, REJECTED, CANCELLED)
        // Les TRANSFER ne sont pas inclus dans les totaux INCOME/EXPENSE
        var currentPeriodCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= startDate &&
                  cf.Date <= endDate,
            cancellationToken);

        var currentCashFlowsList = currentPeriodCashFlows.ToList();

        // 3. Calculer les totaux (INCOME et EXPENSE uniquement, pas TRANSFER)
        var incomeCashFlows = currentCashFlowsList.Where(cf => cf.Type == CashFlowType.INCOME).ToList();
        var expenseCashFlows = currentCashFlowsList.Where(cf => cf.Type == CashFlowType.EXPENSE).ToList();

        var totalIncome = incomeCashFlows.Sum(cf => cf.Amount);
        var totalExpense = expenseCashFlows.Sum(cf => cf.Amount);
        var netBalance = totalIncome - totalExpense;

        var incomeCount = incomeCashFlows.Count;
        var expenseCount = expenseCashFlows.Count;

        // 4. Calculer la repartition par categorie pour les INCOME
        var incomeByCategory = CalculateCategoryBreakdown(incomeCashFlows, categoriesDict, totalIncome);

        // 5. Calculer la repartition par categorie pour les EXPENSE
        var expenseByCategory = CalculateCategoryBreakdown(expenseCashFlows, categoriesDict, totalExpense);

        // 6. Calculer la comparaison avec la periode precedente si demandee
        PeriodComparisonDto? comparison = null;
        if (query.ComparePrevious)
        {
            comparison = await CalculatePeriodComparisonAsync(
                query.ApplicationId,
                query.BoutiqueId,
                startDate,
                endDate,
                totalIncome,
                totalExpense,
                netBalance,
                cancellationToken);
        }

        return new GetCashFlowStatementResponse(
            StartDate: query.StartDate,
            EndDate: query.EndDate,
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            NetBalance: netBalance,
            IncomeCount: incomeCount,
            ExpenseCount: expenseCount,
            IncomeByCategory: incomeByCategory,
            ExpenseByCategory: expenseByCategory,
            Comparison: comparison
        );
    }

    /// <summary>
    /// Calcule la repartition des flux par categorie
    /// </summary>
    private static IReadOnlyList<CategoryBreakdownDto> CalculateCategoryBreakdown(
        List<CashFlow> cashFlows,
        Dictionary<string, Category> categoriesDict,
        decimal total)
    {
        if (cashFlows.Count == 0 || total == 0)
        {
            return Array.Empty<CategoryBreakdownDto>();
        }

        return cashFlows
            .GroupBy(cf => cf.CategoryId)
            .Select(g =>
            {
                var categoryId = g.Key;
                var categoryName = categoriesDict.TryGetValue(categoryId, out var category)
                    ? category.Name
                    : "Non categorise";

                var amount = g.Sum(cf => cf.Amount);
                var percentage = total > 0 ? Math.Round((amount / total) * 100, 2) : 0;
                var transactionCount = g.Count();

                // Convertir le categoryId en Guid
                Guid.TryParse(categoryId, out var categoryGuid);

                return new CategoryBreakdownDto(
                    CategoryId: categoryGuid,
                    CategoryName: categoryName,
                    Amount: amount,
                    Percentage: percentage,
                    TransactionCount: transactionCount
                );
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    /// <summary>
    /// Calcule la comparaison avec la periode precedente
    /// La periode precedente a la meme duree que la periode courante
    /// </summary>
    private async Task<PeriodComparisonDto> CalculatePeriodComparisonAsync(
        string applicationId,
        string boutiqueId,
        DateTime currentStartDate,
        DateTime currentEndDate,
        decimal currentTotalIncome,
        decimal currentTotalExpense,
        decimal currentNetBalance,
        CancellationToken cancellationToken)
    {
        // Calculer la duree de la periode courante
        var periodDuration = currentEndDate - currentStartDate;

        // Calculer les dates de la periode precedente
        var previousEndDate = currentStartDate.AddTicks(-1);
        var previousStartDate = previousEndDate - periodDuration;

        // Recuperer les flux APPROVED de la periode precedente
        var previousPeriodCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == applicationId &&
                  cf.BoutiqueId == boutiqueId &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= previousStartDate &&
                  cf.Date <= previousEndDate,
            cancellationToken);

        var previousCashFlowsList = previousPeriodCashFlows.ToList();

        // Calculer les totaux de la periode precedente
        var previousTotalIncome = previousCashFlowsList
            .Where(cf => cf.Type == CashFlowType.INCOME)
            .Sum(cf => cf.Amount);

        var previousTotalExpense = previousCashFlowsList
            .Where(cf => cf.Type == CashFlowType.EXPENSE)
            .Sum(cf => cf.Amount);

        var previousNetBalance = previousTotalIncome - previousTotalExpense;

        // Calculer les variations en pourcentage
        var incomeVariation = CalculatePercentageVariation(previousTotalIncome, currentTotalIncome);
        var expenseVariation = CalculatePercentageVariation(previousTotalExpense, currentTotalExpense);
        var netBalanceVariation = CalculatePercentageVariation(previousNetBalance, currentNetBalance);

        return new PeriodComparisonDto(
            PreviousStartDate: previousStartDate.Date,
            PreviousEndDate: previousEndDate.Date,
            PreviousTotalIncome: previousTotalIncome,
            PreviousTotalExpense: previousTotalExpense,
            PreviousNetBalance: previousNetBalance,
            IncomeVariation: incomeVariation,
            ExpenseVariation: expenseVariation,
            NetBalanceVariation: netBalanceVariation
        );
    }

    /// <summary>
    /// Calcule la variation en pourcentage entre deux valeurs
    /// Gere correctement les cas ou les valeurs peuvent etre negatives (ex: netBalance)
    /// </summary>
    private static decimal CalculatePercentageVariation(decimal previousValue, decimal currentValue)
    {
        if (previousValue == 0)
        {
            // Si la valeur precedente est 0:
            // - Si currentValue > 0: variation = +100%
            // - Si currentValue < 0: variation = -100%
            // - Si currentValue == 0: variation = 0%
            if (currentValue > 0) return 100m;
            if (currentValue < 0) return -100m;
            return 0m;
        }

        return Math.Round(((currentValue - previousValue) / Math.Abs(previousValue)) * 100, 2);
    }
}
