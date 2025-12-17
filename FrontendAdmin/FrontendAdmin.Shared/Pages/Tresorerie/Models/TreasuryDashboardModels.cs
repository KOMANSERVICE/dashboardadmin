namespace FrontendAdmin.Shared.Pages.Tresorerie.Models;

/// <summary>
/// DTO pour le tableau de bord de tresorerie
/// </summary>
public record TreasuryDashboardDto(
    decimal TotalBalance,
    Dictionary<string, decimal> BalanceByType,
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    decimal NetBalance,
    int PendingCount,
    decimal PendingAmount,
    IReadOnlyList<AccountAlertDto> Alerts,
    IReadOnlyList<BalanceEvolutionDto> Evolution,
    DateTime CalculatedAt
);

/// <summary>
/// DTO pour une alerte de compte
/// </summary>
public record AccountAlertDto(
    Guid AccountId,
    string AccountName,
    string Type,
    decimal CurrentBalance,
    decimal? AlertThreshold,
    string AlertType
);

/// <summary>
/// DTO pour l'evolution du solde
/// </summary>
public record BalanceEvolutionDto(
    DateTime Date,
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpense
);

/// <summary>
/// Reponse wrapper pour le tableau de bord
/// </summary>
public record TreasuryDashboardResponse(TreasuryDashboardDto Dashboard);
