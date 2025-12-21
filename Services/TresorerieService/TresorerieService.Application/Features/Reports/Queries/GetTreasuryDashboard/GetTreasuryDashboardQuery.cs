using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Reports.DTOs;

namespace TresorerieService.Application.Features.Reports.Queries.GetTreasuryDashboard;

/// <summary>
/// Query pour recuperer le tableau de bord de tresorerie
/// </summary>
public record GetTreasuryDashboardQuery(
    string ApplicationId,
    string BoutiqueId
) : IQuery<TreasuryDashboardDto>;
