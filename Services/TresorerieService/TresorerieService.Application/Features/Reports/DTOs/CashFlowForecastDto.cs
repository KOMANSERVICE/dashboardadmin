namespace TresorerieService.Application.Features.Reports.DTOs;

/// <summary>
/// DTO principal pour les previsions de tresorerie
/// US-036: Previsions de tresorerie
/// </summary>
public record CashFlowForecastDto(
    // Periode
    DateTime StartDate,
    DateTime EndDate,
    int Days,

    // Devise
    string Currency,

    // Soldes
    decimal CurrentBalance,
    decimal ForecastedEndBalance,

    // Risques
    bool HasNegativeRisk,
    IReadOnlyList<CriticalDateDto> CriticalDates,

    // Previsions journalieres
    IReadOnlyList<DailyForecastDto> DailyForecast,

    // Resume
    ForecastSummaryDto Summary,

    // Options
    bool IncludePending,

    // Metadata
    DateTime CalculatedAt
);

/// <summary>
/// DTO pour une date critique (solde negatif ou sous le seuil d'alerte)
/// </summary>
public record CriticalDateDto(
    DateTime Date,
    decimal ForecastedBalance,
    string Reason
);

/// <summary>
/// DTO pour les previsions d'un jour
/// </summary>
public record DailyForecastDto(
    DateTime Date,
    decimal OpeningBalance,
    decimal Income,
    decimal Expense,
    decimal PendingIncome,
    decimal PendingExpense,
    decimal RecurringIncome,
    decimal RecurringExpense,
    decimal ClosingBalance,
    bool IsNegative,
    bool IsCritical
);

/// <summary>
/// DTO pour le resume des previsions
/// </summary>
public record ForecastSummaryDto(
    decimal TotalForecastedIncome,
    decimal TotalForecastedExpense,
    decimal TotalRecurringIncome,
    decimal TotalRecurringExpense,
    decimal TotalPendingIncome,
    decimal TotalPendingExpense,
    decimal NetVariation
);
