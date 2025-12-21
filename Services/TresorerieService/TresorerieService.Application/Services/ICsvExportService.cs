using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Services;

/// <summary>
/// Service d'export au format CSV
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exporte une liste de flux de tresorerie au format CSV
    /// </summary>
    /// <param name="cashFlows">Liste des flux a exporter</param>
    /// <param name="columns">Colonnes a inclure (null = toutes)</param>
    /// <returns>Stream contenant le fichier CSV</returns>
    Task<Stream> ExportAsync(IEnumerable<CashFlowExportDto> cashFlows, string[]? columns = null);
}
