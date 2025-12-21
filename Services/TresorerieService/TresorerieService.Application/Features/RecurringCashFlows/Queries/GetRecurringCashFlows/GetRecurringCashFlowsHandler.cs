using TresorerieService.Application.Features.RecurringCashFlows.DTOs;

namespace TresorerieService.Application.Features.RecurringCashFlows.Queries.GetRecurringCashFlows;

public class GetRecurringCashFlowsHandler(
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository
) : IQueryHandler<GetRecurringCashFlowsQuery, GetRecurringCashFlowsResponse>
{
    public async Task<GetRecurringCashFlowsResponse> Handle(
        GetRecurringCashFlowsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer les categories pour le mapping
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == query.ApplicationId,
            cancellationToken);
        var categoryDict = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

        // Recuperer les comptes pour le mapping
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == query.ApplicationId
                 && a.BoutiqueId == query.BoutiqueId,
            cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id, a => a.Name);

        // Recuperer les flux recurrents de la boutique
        var recurringCashFlows = await recurringCashFlowRepository.GetByConditionAsync(
            rcf => rcf.ApplicationId == query.ApplicationId
                   && rcf.BoutiqueId == query.BoutiqueId,
            cancellationToken);

        IEnumerable<RecurringCashFlow> filteredFlows = recurringCashFlows;

        // Appliquer le filtre IsActive (true par defaut)
        if (query.IsActive.HasValue)
        {
            filteredFlows = filteredFlows.Where(rcf => rcf.IsActive == query.IsActive.Value);
        }

        // Appliquer le filtre Type si specifie
        if (query.Type.HasValue)
        {
            filteredFlows = filteredFlows.Where(rcf => rcf.Type == query.Type.Value);
        }

        var flowsList = filteredFlows.ToList();

        // Calculer le montant total mensuel estime sur TOUS les flux actifs de la boutique
        // (independamment du filtre demande par l'utilisateur)
        var activeFlowsForCalculation = recurringCashFlows.Where(f => f.IsActive);
        var estimatedMonthlyTotal = CalculateEstimatedMonthlyTotal(activeFlowsForCalculation);

        // Mapper vers les DTOs
        var dtos = flowsList.Select(rcf => new RecurringCashFlowDTO(
            Id: rcf.Id,
            ApplicationId: rcf.ApplicationId,
            BoutiqueId: rcf.BoutiqueId,
            Type: rcf.Type,
            CategoryId: rcf.CategoryId,
            CategoryName: categoryDict.TryGetValue(rcf.CategoryId, out var catName) ? catName : "Inconnu",
            Label: rcf.Label,
            Description: rcf.Description,
            Amount: rcf.Amount,
            AccountId: rcf.AccountId,
            AccountName: accountDict.TryGetValue(rcf.AccountId, out var accName) ? accName : "Inconnu",
            PaymentMethod: rcf.PaymentMethod,
            ThirdPartyName: rcf.ThirdPartyName,
            Frequency: rcf.Frequency,
            Interval: rcf.Interval,
            DayOfMonth: rcf.DayOfMonth,
            DayOfWeek: rcf.DayOfWeek,
            StartDate: rcf.StartDate,
            EndDate: rcf.EndDate,
            NextOccurrence: rcf.NextOccurrence,
            AutoValidate: rcf.AutoValidate,
            IsActive: rcf.IsActive,
            LastGeneratedAt: rcf.LastGeneratedAt,
            CreatedAt: rcf.CreatedAt,
            CreatedBy: rcf.CreatedBy
        )).ToList();

        return new GetRecurringCashFlowsResponse(
            RecurringCashFlows: dtos,
            TotalCount: dtos.Count,
            EstimatedMonthlyTotal: estimatedMonthlyTotal
        );
    }

    /// <summary>
    /// Calcule le montant total mensuel estime base sur la frequence et l'intervalle.
    /// Formule:
    /// - DAILY: amount * 30 / interval
    /// - WEEKLY: amount * 4.33 / interval
    /// - MONTHLY: amount / interval
    /// - QUARTERLY: amount / (3 * interval)
    /// - YEARLY: amount / (12 * interval)
    ///
    /// Les INCOME sont positifs, les EXPENSE sont negatifs dans le total.
    /// </summary>
    private static decimal CalculateEstimatedMonthlyTotal(IEnumerable<RecurringCashFlow> flows)
    {
        decimal total = 0m;

        foreach (var flow in flows)
        {
            var monthlyAmount = CalculateMonthlyAmount(flow.Amount, flow.Frequency, flow.Interval);

            // Les INCOME ajoutent au total, les EXPENSE soustraient
            if (flow.Type == CashFlowType.INCOME)
            {
                total += monthlyAmount;
            }
            else if (flow.Type == CashFlowType.EXPENSE)
            {
                total -= monthlyAmount;
            }
        }

        return Math.Round(total, 2);
    }

    private static decimal CalculateMonthlyAmount(decimal amount, RecurringFrequency frequency, int interval)
    {
        // Eviter la division par zero
        if (interval <= 0) interval = 1;

        return frequency switch
        {
            RecurringFrequency.DAILY => amount * 30m / interval,
            RecurringFrequency.WEEKLY => amount * 4.33m / interval,
            RecurringFrequency.MONTHLY => amount / interval,
            RecurringFrequency.QUARTERLY => amount / (3m * interval),
            RecurringFrequency.YEARLY => amount / (12m * interval),
            _ => amount
        };
    }
}
