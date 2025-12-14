using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Accounts.DTOs;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccountBalance;

/// <summary>
/// Query pour recuperer le solde en temps reel d'un compte de tresorerie
/// Inclut les variations depuis le debut du mois et de la journee
/// </summary>
public record GetAccountBalanceQuery(
    string ApplicationId,
    string BoutiqueId,
    Guid AccountId
) : IQuery<AccountBalanceDto>;
