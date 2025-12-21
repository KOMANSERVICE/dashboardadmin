using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccounts;

/// <summary>
/// Query pour récupérer la liste des comptes de trésorerie
/// </summary>
public record GetAccountsQuery(
    string ApplicationId,
    string BoutiqueId,
    bool IncludeInactive = false,
    AccountType? Type = null
) : IQuery<GetAccountsResponse>;
