using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Reports.DTOs;

/// <summary>
/// DTO pour une alerte de compte (solde bas)
/// </summary>
public record AccountAlertDto(
    Guid AccountId,
    string AccountName,
    AccountType Type,
    decimal CurrentBalance,
    decimal? AlertThreshold,
    string AlertType // "LOW_BALANCE" | "BUDGET_EXCEEDED"
);
