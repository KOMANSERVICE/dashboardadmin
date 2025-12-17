using IDR.Library.BuildingBlocks.CQRS;

namespace TresorerieService.Application.Features.Reports.Queries.GetCashFlowStatement;

/// <summary>
/// Query pour recuperer l'etat des flux de tresorerie (cash flow statement)
/// US-035: Etat des flux de tresorerie
/// </summary>
public record GetCashFlowStatementQuery(
    string ApplicationId,
    string BoutiqueId,
    DateTime StartDate,
    DateTime EndDate,
    bool ComparePrevious = false
) : IQuery<GetCashFlowStatementResponse>;
