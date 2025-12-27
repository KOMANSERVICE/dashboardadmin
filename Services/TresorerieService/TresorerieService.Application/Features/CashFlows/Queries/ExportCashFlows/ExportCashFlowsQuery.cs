namespace TresorerieService.Application.Features.CashFlows.Queries.ExportCashFlows;

/// <summary>
/// Format d'export disponible
/// </summary>
public enum ExportFormat
{
    Csv,
    Excel
}

/// <summary>
/// Query pour exporter les flux de tresorerie au format CSV ou Excel
/// </summary>
public record ExportCashFlowsQuery(
    // Identifiants obligatoires
    string ApplicationId,
    string BoutiqueId,
    bool IsManager,

    // Format d'export
    ExportFormat Format = ExportFormat.Csv,

    // Colonnes a exporter (null = toutes)
    string[]? Columns = null,

    // Filtres optionnels (identiques a GetCashFlowsQuery)
    CashFlowType? Type = null,
    CashFlowStatus? Status = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,

    // Recherche
    string? Search = null
) : IQuery<ExportCashFlowsResponse>;
