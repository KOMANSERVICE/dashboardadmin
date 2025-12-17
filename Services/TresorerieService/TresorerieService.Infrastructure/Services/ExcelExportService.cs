using System.Globalization;
using ClosedXML.Excel;
using TresorerieService.Application.Features.CashFlows.DTOs;
using TresorerieService.Application.Services;

namespace TresorerieService.Infrastructure.Services;

/// <summary>
/// Implementation du service d'export Excel utilisant ClosedXML
/// </summary>
public class ExcelExportService : IExcelExportService
{
    public Task<Stream> ExportAsync(IEnumerable<CashFlowExportDto> cashFlows, string[]? columns = null)
    {
        var selectedColumns = columns ?? ExportColumns.AllColumns;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Flux de tresorerie");

        // Ecrire les en-tetes avec style
        for (var i = 0; i < selectedColumns.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = ExportColumns.GetDisplayName(selectedColumns[i]);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        // Ecrire les donnees
        var row = 2;
        foreach (var cashFlow in cashFlows)
        {
            for (var col = 0; col < selectedColumns.Length; col++)
            {
                var cell = worksheet.Cell(row, col + 1);
                SetCellValue(cell, cashFlow, selectedColumns[col]);
            }
            row++;
        }

        // Ajuster la largeur des colonnes
        worksheet.Columns().AdjustToContents();

        // Sauvegarder dans un stream
        var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        memoryStream.Position = 0;

        return Task.FromResult<Stream>(memoryStream);
    }

    private static void SetCellValue(IXLCell cell, CashFlowExportDto cashFlow, string column)
    {
        switch (column.ToLowerInvariant())
        {
            case ExportColumns.Reference:
                cell.Value = cashFlow.Reference;
                break;
            case ExportColumns.Type:
                cell.Value = cashFlow.Type;
                break;
            case ExportColumns.Statut:
                cell.Value = cashFlow.Statut;
                break;
            case ExportColumns.Categorie:
                cell.Value = cashFlow.Categorie;
                break;
            case ExportColumns.Libelle:
                cell.Value = cashFlow.Libelle;
                break;
            case ExportColumns.Description:
                cell.Value = cashFlow.Description ?? string.Empty;
                break;
            case ExportColumns.Montant:
                cell.Value = cashFlow.Montant;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case ExportColumns.Taxes:
                cell.Value = cashFlow.Taxes;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case ExportColumns.TauxTVA:
                cell.Value = cashFlow.TauxTVA;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case ExportColumns.Devise:
                cell.Value = cashFlow.Devise;
                break;
            case ExportColumns.Compte:
                cell.Value = cashFlow.Compte;
                break;
            case ExportColumns.CompteDestination:
                cell.Value = cashFlow.CompteDestination ?? string.Empty;
                break;
            case ExportColumns.ModePaiement:
                cell.Value = cashFlow.ModePaiement;
                break;
            case ExportColumns.Date:
                cell.Value = cashFlow.Date;
                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                break;
            case ExportColumns.TypeDeTiers:
                cell.Value = cashFlow.TypeDeTiers ?? string.Empty;
                break;
            case ExportColumns.NomDuTiers:
                cell.Value = cashFlow.NomDuTiers ?? string.Empty;
                break;
            case ExportColumns.IdTiers:
                cell.Value = cashFlow.IdTiers ?? string.Empty;
                break;
            case ExportColumns.CreeLe:
                cell.Value = cashFlow.CreeLe;
                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                break;
            case ExportColumns.CreePar:
                cell.Value = cashFlow.CreePar;
                break;
            case ExportColumns.SoumisLe:
                if (cashFlow.SoumisLe.HasValue)
                {
                    cell.Value = cashFlow.SoumisLe.Value;
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                }
                break;
            case ExportColumns.SoumisPar:
                cell.Value = cashFlow.SoumisPar ?? string.Empty;
                break;
            case ExportColumns.ValideLe:
                if (cashFlow.ValideLe.HasValue)
                {
                    cell.Value = cashFlow.ValideLe.Value;
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                }
                break;
            case ExportColumns.ValidePar:
                cell.Value = cashFlow.ValidePar ?? string.Empty;
                break;
            case ExportColumns.RaisonDeRejet:
                cell.Value = cashFlow.RaisonDeRejet ?? string.Empty;
                break;
            case ExportColumns.Rapproche:
                cell.Value = cashFlow.Rapproche;
                break;
            case ExportColumns.ReferenceBancaire:
                cell.Value = cashFlow.ReferenceBancaire ?? string.Empty;
                break;
        }
    }
}
