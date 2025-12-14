using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccountDetail;

/// <summary>
/// Reponse pour le detail d'un compte de tresorerie
/// Contient toutes les informations du compte, les totaux et les derniers mouvements
/// </summary>
public record GetAccountDetailResponse(
    // Informations de base du compte
    Guid Id,
    string Name,
    string? Description,
    AccountType Type,
    string? AccountNumber,
    string? BankName,

    // Soldes
    decimal InitialBalance,
    decimal CurrentBalance,
    string Currency,

    // Configuration
    bool IsActive,
    bool IsDefault,
    decimal? OverdraftLimit,
    decimal? AlertThreshold,
    bool IsInAlert,

    // Totaux des mouvements (sur mouvements APPROVED uniquement)
    decimal TotalIncome,
    decimal TotalExpense,

    // Derniers mouvements (20 derniers)
    IReadOnlyList<CashFlowMovementDto> RecentMovements,

    // Evolution du solde (optionnel, si periode specifiee)
    IReadOnlyList<BalanceEvolutionDto>? BalanceEvolution,

    // Metadonnees
    DateTime CreatedAt,
    DateTime UpdatedAt
);
