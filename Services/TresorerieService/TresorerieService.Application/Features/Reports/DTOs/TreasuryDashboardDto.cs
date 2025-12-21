using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Reports.DTOs;

/// <summary>
/// DTO pour le tableau de bord de tresorerie
/// </summary>
public record TreasuryDashboardDto(
    // Totaux
    decimal TotalBalance,                                   // Tous comptes actifs
    Dictionary<AccountType, decimal> BalanceByType,         // Par type de compte

    // Mois courant
    decimal MonthlyIncome,                                  // Revenus du mois (APPROVED)
    decimal MonthlyExpense,                                 // Depenses du mois (APPROVED)
    decimal NetBalance,                                     // Revenus - Depenses

    // Flux en attente
    int PendingCount,                                       // Nombre de flux PENDING
    decimal PendingAmount,                                  // Montant total PENDING

    // Alertes
    IReadOnlyList<AccountAlertDto> Alerts,                 // Comptes en alerte

    // Evolution sur 6 mois
    IReadOnlyList<BalanceEvolutionDto> Evolution,          // Historique mensuel

    // Metadata
    DateTime CalculatedAt
);
