using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.DTOs;

public record AccountDTO(
    Guid Id,
    string ApplicationId,
    string BoutiqueId,
    string Name,
    string? Description,
    AccountType Type,
    string? AccountNumber,
    string? BankName,
    decimal InitialBalance,
    decimal CurrentBalance,
    string Currency,
    bool IsActive,
    bool IsDefault,
    decimal? OverdraftLimit,
    decimal? AlertThreshold,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
