using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccountDetail;

/// <summary>
/// Query pour recuperer le detail d'un compte de tresorerie
/// </summary>
public record GetAccountDetailQuery(
    string ApplicationId,
    string BoutiqueId,
    Guid AccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IQuery<GetAccountDetailResponse>;
