namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowStatement;

/// <summary>
/// Response pour l'etat des flux de tresorerie
/// </summary>
public record GetCashFlowStatementResponse(
    // Periode
    DateTime StartDate,
    DateTime EndDate,

    // Totaux
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,

    // Nombre de flux
    int IncomeCount,
    int ExpenseCount,

    // Repartition par categorie
    IReadOnlyList<CategoryBreakdownDto> IncomeByCategory,
    IReadOnlyList<CategoryBreakdownDto> ExpenseByCategory,

    // Comparaison periode precedente (optionnel)
    PeriodComparisonDto? Comparison
);

/// <summary>
/// DTO pour la repartition par categorie
/// </summary>
public record CategoryBreakdownDto(
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    decimal Percentage,
    int TransactionCount
);

/// <summary>
/// DTO pour la comparaison avec la periode precedente
/// </summary>
public record PeriodComparisonDto(
    DateTime PreviousStartDate,
    DateTime PreviousEndDate,
    decimal PreviousTotalIncome,
    decimal PreviousTotalExpense,
    decimal PreviousNetBalance,
    decimal IncomeVariation,      // % variation
    decimal ExpenseVariation,     // % variation
    decimal NetBalanceVariation   // % variation
);
