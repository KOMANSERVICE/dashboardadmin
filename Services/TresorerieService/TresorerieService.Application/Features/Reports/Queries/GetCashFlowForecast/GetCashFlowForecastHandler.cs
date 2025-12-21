using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Reports.DTOs;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowForecast;

/// <summary>
/// Handler pour recuperer les previsions de tresorerie
/// US-036: Previsions de tresorerie
/// </summary>
public class GetCashFlowForecastHandler(
    IGenericRepository<Account> accountRepository,
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository)
    : IQueryHandler<GetCashFlowForecastQuery, CashFlowForecastDto>
{
    public async Task<CashFlowForecastDto> Handle(
        GetCashFlowForecastQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startDate = now.Date;
        var endDate = startDate.AddDays(query.Days - 1);

        // 1. Recuperer le compte principal de la boutique (le compte par defaut ou le premier actif)
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == query.ApplicationId &&
                 a.BoutiqueId == query.BoutiqueId &&
                 a.IsActive,
            cancellationToken);

        var accountsList = accounts.ToList();
        if (accountsList.Count == 0)
        {
            throw new NotFoundException("Aucun compte de tresorerie trouve");
        }

        // Utiliser le compte par defaut ou le premier compte actif
        var primaryAccount = accountsList.FirstOrDefault(a => a.IsDefault) ?? accountsList.First();
        var currentBalance = accountsList.Sum(a => a.CurrentBalance);
        var alertThreshold = primaryAccount.AlertThreshold ?? 0;
        var currency = primaryAccount.Currency;

        // 2. Recuperer les flux PENDING futurs si demande
        var pendingCashFlows = new List<CashFlow>();
        if (query.IncludePending)
        {
            var pending = await cashFlowRepository.GetByConditionAsync(
                cf => cf.ApplicationId == query.ApplicationId &&
                      cf.BoutiqueId == query.BoutiqueId &&
                      cf.Status == CashFlowStatus.PENDING &&
                      cf.Date >= startDate &&
                      cf.Date <= endDate,
                cancellationToken);
            pendingCashFlows = pending.ToList();
        }

        // 3. Recuperer les flux recurrents actifs
        var recurringFlows = await recurringCashFlowRepository.GetByConditionAsync(
            rcf => rcf.ApplicationId == query.ApplicationId &&
                   rcf.BoutiqueId == query.BoutiqueId &&
                   rcf.IsActive &&
                   rcf.StartDate <= endDate &&
                   (rcf.EndDate == null || rcf.EndDate >= startDate),
            cancellationToken);

        var recurringFlowsList = recurringFlows.ToList();

        // 4. Calculer les previsions journalieres
        var dailyForecasts = new List<DailyForecastDto>();
        var criticalDates = new List<CriticalDateDto>();
        var hasNegativeRisk = false;

        // Totaux pour le resume
        decimal totalRecurringIncome = 0;
        decimal totalRecurringExpense = 0;
        decimal totalPendingIncome = 0;
        decimal totalPendingExpense = 0;

        var runningBalance = currentBalance;

        for (int dayIndex = 0; dayIndex < query.Days; dayIndex++)
        {
            var currentDay = startDate.AddDays(dayIndex);
            var openingBalance = runningBalance;

            // Flux reguliers (deja approuves pour la journee - normalement pas de flux futurs approuves)
            decimal income = 0;
            decimal expense = 0;

            // Flux PENDING du jour
            decimal pendingIncome = 0;
            decimal pendingExpense = 0;

            if (query.IncludePending)
            {
                var dayPendingFlows = pendingCashFlows
                    .Where(cf => cf.Date.Date == currentDay.Date)
                    .ToList();

                pendingIncome = dayPendingFlows
                    .Where(cf => cf.Type == CashFlowType.INCOME)
                    .Sum(cf => cf.Amount);

                pendingExpense = dayPendingFlows
                    .Where(cf => cf.Type == CashFlowType.EXPENSE)
                    .Sum(cf => cf.Amount);

                totalPendingIncome += pendingIncome;
                totalPendingExpense += pendingExpense;
            }

            // Flux recurrents du jour
            var (recurringIncome, recurringExpense, recurringReasons) = CalculateRecurringFlowsForDay(
                recurringFlowsList, currentDay);

            totalRecurringIncome += recurringIncome;
            totalRecurringExpense += recurringExpense;

            // Calculer le solde de fermeture
            var closingBalance = openingBalance + income + pendingIncome + recurringIncome
                                                - expense - pendingExpense - recurringExpense;

            var isNegative = closingBalance < 0;
            var isCritical = closingBalance < alertThreshold;

            if (isNegative)
            {
                hasNegativeRisk = true;
            }

            // Ajouter aux dates critiques si necessaire
            if (isNegative || isCritical)
            {
                var reasons = new List<string>();
                if (recurringReasons.Any())
                {
                    reasons.AddRange(recurringReasons);
                }
                if (pendingExpense > 0)
                {
                    reasons.Add("Flux en attente");
                }

                var reason = reasons.Any() ? string.Join(", ", reasons) : "Solde insuffisant";

                criticalDates.Add(new CriticalDateDto(
                    Date: currentDay,
                    ForecastedBalance: closingBalance,
                    Reason: reason
                ));
            }

            dailyForecasts.Add(new DailyForecastDto(
                Date: currentDay,
                OpeningBalance: openingBalance,
                Income: income,
                Expense: expense,
                PendingIncome: pendingIncome,
                PendingExpense: pendingExpense,
                RecurringIncome: recurringIncome,
                RecurringExpense: recurringExpense,
                ClosingBalance: closingBalance,
                IsNegative: isNegative,
                IsCritical: isCritical
            ));

            // Le solde de fermeture devient le solde d'ouverture du jour suivant
            runningBalance = closingBalance;
        }

        // Calculer le resume
        var totalForecastedIncome = totalRecurringIncome + totalPendingIncome;
        var totalForecastedExpense = totalRecurringExpense + totalPendingExpense;
        var netVariation = totalForecastedIncome - totalForecastedExpense;

        var summary = new ForecastSummaryDto(
            TotalForecastedIncome: totalForecastedIncome,
            TotalForecastedExpense: totalForecastedExpense,
            TotalRecurringIncome: totalRecurringIncome,
            TotalRecurringExpense: totalRecurringExpense,
            TotalPendingIncome: totalPendingIncome,
            TotalPendingExpense: totalPendingExpense,
            NetVariation: netVariation
        );

        // Solde prevu en fin de periode
        var forecastedEndBalance = dailyForecasts.LastOrDefault()?.ClosingBalance ?? currentBalance;

        return new CashFlowForecastDto(
            StartDate: startDate,
            EndDate: endDate,
            Days: query.Days,
            Currency: currency,
            CurrentBalance: currentBalance,
            ForecastedEndBalance: forecastedEndBalance,
            HasNegativeRisk: hasNegativeRisk,
            CriticalDates: criticalDates,
            DailyForecast: dailyForecasts,
            Summary: summary,
            IncludePending: query.IncludePending,
            CalculatedAt: now
        );
    }

    /// <summary>
    /// Calcule les flux recurrents pour un jour donne
    /// </summary>
    private (decimal Income, decimal Expense, List<string> Reasons) CalculateRecurringFlowsForDay(
        List<RecurringCashFlow> recurringFlows,
        DateTime day)
    {
        decimal income = 0;
        decimal expense = 0;
        var reasons = new List<string>();

        foreach (var rcf in recurringFlows)
        {
            // Verifier si ce flux recurrent tombe sur ce jour
            if (!IsRecurringFlowDueOnDay(rcf, day))
            {
                continue;
            }

            if (rcf.Type == CashFlowType.INCOME)
            {
                income += rcf.Amount;
            }
            else if (rcf.Type == CashFlowType.EXPENSE)
            {
                expense += rcf.Amount;
                reasons.Add(rcf.Label);
            }
        }

        return (income, expense, reasons);
    }

    /// <summary>
    /// Verifie si un flux recurrent est du pour un jour donne
    /// </summary>
    private bool IsRecurringFlowDueOnDay(RecurringCashFlow rcf, DateTime day)
    {
        // Verifier les bornes de dates
        if (day.Date < rcf.StartDate.Date)
            return false;

        if (rcf.EndDate.HasValue && day.Date > rcf.EndDate.Value.Date)
            return false;

        // Calculer les occurrences en fonction de la frequence
        return rcf.Frequency switch
        {
            RecurringFrequency.DAILY => IsDailyOccurrence(rcf, day),
            RecurringFrequency.WEEKLY => IsWeeklyOccurrence(rcf, day),
            RecurringFrequency.MONTHLY => IsMonthlyOccurrence(rcf, day),
            RecurringFrequency.QUARTERLY => IsQuarterlyOccurrence(rcf, day),
            RecurringFrequency.YEARLY => IsYearlyOccurrence(rcf, day),
            _ => false
        };
    }

    /// <summary>
    /// Verifie si c'est une occurrence journaliere
    /// </summary>
    private bool IsDailyOccurrence(RecurringCashFlow rcf, DateTime day)
    {
        var daysSinceStart = (day.Date - rcf.StartDate.Date).Days;
        return daysSinceStart >= 0 && daysSinceStart % rcf.Interval == 0;
    }

    /// <summary>
    /// Verifie si c'est une occurrence hebdomadaire
    /// </summary>
    private bool IsWeeklyOccurrence(RecurringCashFlow rcf, DateTime day)
    {
        // Verifier le jour de la semaine (1 = Lundi, 7 = Dimanche)
        // DayOfWeek de .NET: Sunday = 0, Monday = 1, ..., Saturday = 6
        // Notre convention: Lundi = 1, ..., Dimanche = 7
        var dotNetDayOfWeek = (int)day.DayOfWeek;
        var ourDayOfWeek = dotNetDayOfWeek == 0 ? 7 : dotNetDayOfWeek; // Dimanche = 7

        if (rcf.DayOfWeek.HasValue)
        {
            // Si DayOfWeek est specifie, verifier que le jour correspond
            if (ourDayOfWeek != rcf.DayOfWeek.Value)
                return false;
        }
        else
        {
            // Si DayOfWeek n'est pas specifie, utiliser le meme jour que la date de debut
            var startDotNetDayOfWeek = (int)rcf.StartDate.DayOfWeek;
            var startOurDayOfWeek = startDotNetDayOfWeek == 0 ? 7 : startDotNetDayOfWeek;
            if (ourDayOfWeek != startOurDayOfWeek)
                return false;
        }

        // Verifier l'intervalle de semaines depuis la date de debut
        var daysSinceStart = (day.Date - rcf.StartDate.Date).Days;
        var weeksSinceStart = daysSinceStart / 7;
        return weeksSinceStart >= 0 && weeksSinceStart % rcf.Interval == 0;
    }

    /// <summary>
    /// Verifie si c'est une occurrence mensuelle
    /// </summary>
    private bool IsMonthlyOccurrence(RecurringCashFlow rcf, DateTime day)
    {
        // Verifier le jour du mois
        if (rcf.DayOfMonth.HasValue)
        {
            var targetDay = rcf.DayOfMonth.Value;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);

            // Si le jour cible est superieur au nombre de jours dans le mois,
            // utiliser le dernier jour du mois
            var effectiveDay = Math.Min(targetDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }
        else
        {
            // Si pas de jour specifie, utiliser le meme jour que la date de debut
            var startDay = rcf.StartDate.Day;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);
            var effectiveDay = Math.Min(startDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }

        // Verifier l'intervalle de mois
        var monthsSinceStart = ((day.Year - rcf.StartDate.Year) * 12) + (day.Month - rcf.StartDate.Month);
        return monthsSinceStart >= 0 && monthsSinceStart % rcf.Interval == 0;
    }

    /// <summary>
    /// Verifie si c'est une occurrence trimestrielle
    /// </summary>
    private bool IsQuarterlyOccurrence(RecurringCashFlow rcf, DateTime day)
    {
        // Une occurrence trimestrielle est comme une occurrence mensuelle avec intervalle = 3 * Interval
        var quarterlyInterval = rcf.Interval * 3;

        // Verifier le jour du mois
        if (rcf.DayOfMonth.HasValue)
        {
            var targetDay = rcf.DayOfMonth.Value;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);
            var effectiveDay = Math.Min(targetDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }
        else
        {
            var startDay = rcf.StartDate.Day;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);
            var effectiveDay = Math.Min(startDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }

        // Verifier l'intervalle de mois (trimestre = 3 mois)
        var monthsSinceStart = ((day.Year - rcf.StartDate.Year) * 12) + (day.Month - rcf.StartDate.Month);
        return monthsSinceStart >= 0 && monthsSinceStart % quarterlyInterval == 0;
    }

    /// <summary>
    /// Verifie si c'est une occurrence annuelle
    /// </summary>
    private bool IsYearlyOccurrence(RecurringCashFlow rcf, DateTime day)
    {
        // Verifier le mois et le jour
        if (day.Month != rcf.StartDate.Month)
            return false;

        // Verifier le jour du mois
        if (rcf.DayOfMonth.HasValue)
        {
            var targetDay = rcf.DayOfMonth.Value;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);
            var effectiveDay = Math.Min(targetDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }
        else
        {
            var startDay = rcf.StartDate.Day;
            var daysInMonth = DateTime.DaysInMonth(day.Year, day.Month);
            var effectiveDay = Math.Min(startDay, daysInMonth);

            if (day.Day != effectiveDay)
                return false;
        }

        // Verifier l'intervalle d'annees
        var yearsSinceStart = day.Year - rcf.StartDate.Year;
        return yearsSinceStart >= 0 && yearsSinceStart % rcf.Interval == 0;
    }
}
