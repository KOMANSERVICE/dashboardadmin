namespace TresorerieService.Application.Features.RecurringCashFlows.Queries.GetRecurringCashFlows;

/// <summary>
/// Query pour recuperer la liste des flux de tresorerie recurrents avec filtres
/// </summary>
public record GetRecurringCashFlowsQuery(
    string ApplicationId,
    string BoutiqueId,

    // Filtre par etat actif (true par defaut)
    bool? IsActive = true,

    // Filtre par type (INCOME/EXPENSE)
    CashFlowType? Type = null
) : IQuery<GetRecurringCashFlowsResponse>;
