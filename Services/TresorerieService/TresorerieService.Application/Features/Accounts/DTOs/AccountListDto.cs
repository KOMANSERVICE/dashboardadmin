using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.DTOs;

/// <summary>
/// DTO pour l'affichage d'un compte dans la liste
/// </summary>
public record AccountListDto(
    Guid Id,
    string Name,
    AccountType Type,
    decimal CurrentBalance,
    string Currency,
    bool IsActive,
    bool IsDefault,
    decimal? AlertThreshold,
    bool IsInAlert
);
