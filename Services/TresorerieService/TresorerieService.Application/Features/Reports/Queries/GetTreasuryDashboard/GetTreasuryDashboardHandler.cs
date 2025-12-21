using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Application.Features.Reports.DTOs;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Reports.Queries.GetTreasuryDashboard;

/// <summary>
/// Handler pour recuperer le tableau de bord de tresorerie
/// </summary>
public class GetTreasuryDashboardHandler(
    IGenericRepository<Account> accountRepository,
    IGenericRepository<CashFlow> cashFlowRepository)
    : IQueryHandler<GetTreasuryDashboardQuery, TreasuryDashboardDto>
{
    public async Task<TreasuryDashboardDto> Handle(
        GetTreasuryDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthsAgo = startOfMonth.AddMonths(-5); // 6 mois incluant le mois courant

        // 1. Recuperer tous les comptes actifs de la boutique
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == query.ApplicationId &&
                 a.BoutiqueId == query.BoutiqueId &&
                 a.IsActive,
            cancellationToken);

        var accountsList = accounts.ToList();

        // 2. Calculer le total de tresorerie (tous comptes)
        var totalBalance = accountsList.Sum(a => a.CurrentBalance);

        // 3. Calculer le total par type de compte
        var balanceByType = accountsList
            .GroupBy(a => a.Type)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.CurrentBalance));

        // S'assurer que tous les types sont presents
        foreach (var accountType in Enum.GetValues<AccountType>())
        {
            if (!balanceByType.ContainsKey(accountType))
            {
                balanceByType[accountType] = 0m;
            }
        }

        // 4. Recuperer les flux du mois courant (APPROVED)
        var monthlyApprovedCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= startOfMonth,
            cancellationToken);

        var monthlyApprovedList = monthlyApprovedCashFlows.ToList();

        // Revenus du mois (INCOME approuves)
        var monthlyIncome = monthlyApprovedList
            .Where(cf => cf.Type == CashFlowType.INCOME)
            .Sum(cf => cf.Amount);

        // Depenses du mois (EXPENSE approuves)
        var monthlyExpense = monthlyApprovedList
            .Where(cf => cf.Type == CashFlowType.EXPENSE)
            .Sum(cf => cf.Amount);

        // Solde net = Revenus - Depenses
        var netBalance = monthlyIncome - monthlyExpense;

        // 5. Recuperer les flux en attente (PENDING)
        var pendingCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Status == CashFlowStatus.PENDING,
            cancellationToken);

        var pendingList = pendingCashFlows.ToList();
        var pendingCount = pendingList.Count;
        var pendingAmount = pendingList.Sum(cf => cf.Amount);

        // 6. Detecter les alertes (solde < seuil)
        var alerts = accountsList
            .Where(a => a.AlertThreshold.HasValue && a.CurrentBalance < a.AlertThreshold.Value)
            .Select(a => new AccountAlertDto(
                AccountId: a.Id,
                AccountName: a.Name,
                Type: a.Type,
                CurrentBalance: a.CurrentBalance,
                AlertThreshold: a.AlertThreshold,
                AlertType: "LOW_BALANCE"
            ))
            .ToList();

        // 7. Calculer l'evolution sur 6 mois
        var evolution = await CalculateEvolutionAsync(
            query.ApplicationId,
            query.BoutiqueId,
            sixMonthsAgo,
            now,
            cancellationToken);

        return new TreasuryDashboardDto(
            TotalBalance: totalBalance,
            BalanceByType: balanceByType,
            MonthlyIncome: monthlyIncome,
            MonthlyExpense: monthlyExpense,
            NetBalance: netBalance,
            PendingCount: pendingCount,
            PendingAmount: pendingAmount,
            Alerts: alerts,
            Evolution: evolution,
            CalculatedAt: now
        );
    }

    /// <summary>
    /// Calcule l'evolution mensuelle du solde sur une periode
    /// </summary>
    private async Task<IReadOnlyList<BalanceEvolutionDto>> CalculateEvolutionAsync(
        string applicationId,
        string boutiqueId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        // Recuperer tous les flux APPROVED sur la periode
        var allCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == applicationId &&
                  cf.BoutiqueId == boutiqueId &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= startDate &&
                  cf.Date <= endDate,
            cancellationToken);

        var cashFlowsList = allCashFlows.ToList();

        // Recuperer le solde total actuel des comptes
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == applicationId &&
                 a.BoutiqueId == boutiqueId &&
                 a.IsActive,
            cancellationToken);

        var currentTotalBalance = accounts.Sum(a => a.CurrentBalance);

        // Grouper par mois et calculer revenus/depenses
        var monthlyData = cashFlowsList
            .GroupBy(cf => new { cf.Date.Year, cf.Date.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalIncome = g.Where(cf => cf.Type == CashFlowType.INCOME).Sum(cf => cf.Amount),
                TotalExpense = g.Where(cf => cf.Type == CashFlowType.EXPENSE).Sum(cf => cf.Amount)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        // Calculer le solde pour chaque mois en remontant depuis le solde actuel
        var evolution = new List<BalanceEvolutionDto>();

        // D'abord, calculer le delta total de tous les flux depuis le debut de la periode
        var totalIncomeInPeriod = cashFlowsList
            .Where(cf => cf.Type == CashFlowType.INCOME)
            .Sum(cf => cf.Amount);
        var totalExpenseInPeriod = cashFlowsList
            .Where(cf => cf.Type == CashFlowType.EXPENSE)
            .Sum(cf => cf.Amount);

        // Solde au debut de la periode = solde actuel - (revenus - depenses de la periode)
        var startingBalance = currentTotalBalance - (totalIncomeInPeriod - totalExpenseInPeriod);

        // Generer les 6 mois
        var currentDate = startDate;
        var cumulativeBalance = startingBalance;

        while (currentDate <= endDate)
        {
            var monthData = monthlyData
                .FirstOrDefault(m => m.Year == currentDate.Year && m.Month == currentDate.Month);

            var monthIncome = monthData?.TotalIncome ?? 0m;
            var monthExpense = monthData?.TotalExpense ?? 0m;

            // Ajouter les mouvements du mois au solde cumulatif
            cumulativeBalance += monthIncome - monthExpense;

            evolution.Add(new BalanceEvolutionDto(
                Date: new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                Balance: cumulativeBalance,
                TotalIncome: monthIncome,
                TotalExpense: monthExpense
            ));

            currentDate = currentDate.AddMonths(1);
        }

        return evolution;
    }
}
