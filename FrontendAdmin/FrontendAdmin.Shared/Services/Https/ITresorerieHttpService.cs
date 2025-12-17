using FrontendAdmin.Shared.Models;
using FrontendAdmin.Shared.Pages.Tresorerie.Models;

namespace FrontendAdmin.Shared.Services.Https;

/// <summary>
/// Service HTTP pour les operations de tresorerie
/// </summary>
public interface ITresorerieHttpService
{
    /// <summary>
    /// Recupere le tableau de bord de tresorerie
    /// </summary>
    [Get("/api/reports/treasury-dashboard")]
    Task<BaseResponse<TreasuryDashboardDto>> GetTreasuryDashboardAsync();
}
