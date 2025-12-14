namespace TresorerieService.Application.Features.Accounts.DTOs;

/// <summary>
/// DTO pour le solde en temps reel d'un compte de tresorerie
/// Inclut les variations depuis le debut du mois et de la journee
/// </summary>
public record AccountBalanceDto(
    Guid AccountId,
    string AccountName,
    decimal CurrentBalance,
    string Currency,

    // Variations
    decimal VariationToday,
    decimal VariationThisMonth,

    // Alerte
    bool IsAlertTriggered,
    decimal? AlertThreshold,

    // Metadonnees
    DateTime CalculatedAt
);
