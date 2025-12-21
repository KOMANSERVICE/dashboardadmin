using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TresorerieService.Application.Features.CashFlows.DTOs;
using TresorerieService.Application.Services;

namespace TresorerieService.Infrastructure.Services;

/// <summary>
/// Implementation du service d'export CSV utilisant CsvHelper
/// </summary>
public class CsvExportService : ICsvExportService
{
    public Task<Stream> ExportAsync(IEnumerable<CashFlowExportDto> cashFlows, string[]? columns = null)
    {
        var selectedColumns = columns ?? ExportColumns.AllColumns;
        var memoryStream = new MemoryStream();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };

        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, config))
        {
            // Ecrire les en-tetes
            foreach (var column in selectedColumns)
            {
                csv.WriteField(ExportColumns.GetDisplayName(column));
            }
            csv.NextRecord();

            // Ecrire les donnees
            foreach (var cashFlow in cashFlows)
            {
                foreach (var column in selectedColumns)
                {
                    var value = GetColumnValue(cashFlow, column);
                    csv.WriteField(value);
                }
                csv.NextRecord();
            }
        }

        memoryStream.Position = 0;
        return Task.FromResult<Stream>(memoryStream);
    }

    private static string? GetColumnValue(CashFlowExportDto cashFlow, string column)
    {
        return column.ToLowerInvariant() switch
        {
            ExportColumns.Reference => cashFlow.Reference,
            ExportColumns.Type => cashFlow.Type,
            ExportColumns.Statut => cashFlow.Statut,
            ExportColumns.Categorie => cashFlow.Categorie,
            ExportColumns.Libelle => cashFlow.Libelle,
            ExportColumns.Description => cashFlow.Description,
            ExportColumns.Montant => cashFlow.Montant.ToString("F2", CultureInfo.InvariantCulture),
            ExportColumns.Taxes => cashFlow.Taxes.ToString("F2", CultureInfo.InvariantCulture),
            ExportColumns.TauxTVA => cashFlow.TauxTVA.ToString("F2", CultureInfo.InvariantCulture),
            ExportColumns.Devise => cashFlow.Devise,
            ExportColumns.Compte => cashFlow.Compte,
            ExportColumns.CompteDestination => cashFlow.CompteDestination,
            ExportColumns.ModePaiement => cashFlow.ModePaiement,
            ExportColumns.Date => cashFlow.Date.ToString("yyyy-MM-dd HH:mm:ss"),
            ExportColumns.TypeDeTiers => cashFlow.TypeDeTiers,
            ExportColumns.NomDuTiers => cashFlow.NomDuTiers,
            ExportColumns.IdTiers => cashFlow.IdTiers,
            ExportColumns.CreeLe => cashFlow.CreeLe.ToString("yyyy-MM-dd HH:mm:ss"),
            ExportColumns.CreePar => cashFlow.CreePar,
            ExportColumns.SoumisLe => cashFlow.SoumisLe?.ToString("yyyy-MM-dd HH:mm:ss"),
            ExportColumns.SoumisPar => cashFlow.SoumisPar,
            ExportColumns.ValideLe => cashFlow.ValideLe?.ToString("yyyy-MM-dd HH:mm:ss"),
            ExportColumns.ValidePar => cashFlow.ValidePar,
            ExportColumns.RaisonDeRejet => cashFlow.RaisonDeRejet,
            ExportColumns.Rapproche => cashFlow.Rapproche,
            ExportColumns.ReferenceBancaire => cashFlow.ReferenceBancaire,
            _ => null
        };
    }
}
