using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Budgets.DTOs;

/// <summary>
/// DTO complet pour le detail d'un budget avec toutes les informations
/// </summary>
public record BudgetDetailDto(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal PercentUsed,
    string Currency,
    BudgetType Type,
    int AlertThreshold,
    bool IsExceeded,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<BudgetExpenseDto> Expenses,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdown,
    IReadOnlyList<TimeSeriesDto> TimeEvolution
);

/// <summary>
/// DTO pour une depense (CashFlow EXPENSE) liee au budget
/// </summary>
public record BudgetExpenseDto(
    Guid Id,
    string Label,
    string? Description,
    decimal Amount,
    string Currency,
    DateTime Date,
    string CategoryId,
    string CategoryName,
    CashFlowStatus Status,
    string? ThirdPartyName
);

/// <summary>
/// DTO pour la repartition par categorie
/// </summary>
public record CategoryBreakdownDto(
    Guid CategoryId,
    string CategoryName,
    string? CategoryIcon,
    decimal Amount,
    decimal Percentage,
    int TransactionCount
);

/// <summary>
/// DTO pour l'evolution dans le temps (par mois)
/// </summary>
public record TimeSeriesDto(
    int Year,
    int Month,
    string MonthLabel,
    decimal Amount,
    decimal CumulativeAmount
);
