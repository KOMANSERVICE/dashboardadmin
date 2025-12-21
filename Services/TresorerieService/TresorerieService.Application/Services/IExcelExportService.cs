using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Services;

/// <summary>
/// Service d'export au format Excel (xlsx)
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// Exporte une liste de flux de tresorerie au format Excel
    /// </summary>
    /// <param name="cashFlows">Liste des flux a exporter</param>
    /// <param name="columns">Colonnes a inclure (null = toutes)</param>
    /// <returns>Stream contenant le fichier Excel</returns>
    Task<Stream> ExportAsync(IEnumerable<CashFlowExportDto> cashFlows, string[]? columns = null);
}
