using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.Reports.DTOs;

namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowForecast;

/// <summary>
/// Query pour recuperer les previsions de tresorerie
/// US-036: Previsions de tresorerie
/// </summary>
public record GetCashFlowForecastQuery(
    string ApplicationId,
    string BoutiqueId,
    int Days = 30,
    bool IncludePending = true
) : IQuery<CashFlowForecastDto>;
