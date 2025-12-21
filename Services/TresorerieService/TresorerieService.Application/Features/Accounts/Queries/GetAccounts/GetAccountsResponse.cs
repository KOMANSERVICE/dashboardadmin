using TresorerieService.Application.Features.Accounts.DTOs;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccounts;

/// <summary>
/// Réponse pour la liste des comptes de trésorerie
/// </summary>
public record GetAccountsResponse(
    IReadOnlyList<AccountListDto> Accounts,
    decimal TotalAvailable,
    int TotalCount
);
