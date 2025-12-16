namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlows;

/// <summary>
/// Query pour recuperer la liste des flux de tresorerie avec filtres, recherche et pagination
/// </summary>
public record GetCashFlowsQuery(
    // Identifiants obligatoires
    string ApplicationId,
    string BoutiqueId,
    string UserId,
    bool IsManager,

    // Filtres optionnels
    CashFlowType? Type = null,
    CashFlowStatus? Status = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,

    // Recherche
    string? Search = null,

    // Pagination
    int Page = 1,
    int PageSize = 20,

    // Tri
    string SortBy = "date",
    string SortOrder = "desc"
) : IQuery<GetCashFlowsResponse>;
