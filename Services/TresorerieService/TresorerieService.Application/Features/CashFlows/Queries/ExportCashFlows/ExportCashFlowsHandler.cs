using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.CashFlows.DTOs;
using TresorerieService.Application.Services;

namespace TresorerieService.Application.Features.CashFlows.Queries.ExportCashFlows;

public class ExportCashFlowsHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    ICsvExportService csvExportService,
    IExcelExportService excelExportService,
    IUserContextService userContextService
) : IQueryHandler<ExportCashFlowsQuery, ExportCashFlowsResponse>
{
    public async Task<ExportCashFlowsResponse> Handle(
        ExportCashFlowsQuery query,
        CancellationToken cancellationToken = default)
    {
        var email = userContextService.GetEmail();
        // Recuperer les categories pour le mapping
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.ApplicationId == query.ApplicationId,
            cancellationToken);
        var categoryDict = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);

        // Recuperer les comptes pour le mapping
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == query.ApplicationId
                 && a.BoutiqueId == query.BoutiqueId,
            cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id, a => a.Name);

        // Recuperer les flux selon les filtres de base
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId
                  && cf.BoutiqueId == query.BoutiqueId,
            cancellationToken);

        // Filtrer selon le role : employe voit ses propres flux, manager voit tous les flux
        IEnumerable<CashFlow> filteredCashFlows = query.IsManager
            ? cashFlows
            : cashFlows.Where(cf => cf.CreatedBy == email);

        // Appliquer les filtres optionnels
        if (query.Type.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Type == query.Type.Value);
        }

        if (query.Status.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Status == query.Status.Value);
        }

        if (query.AccountId.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf =>
                cf.AccountId == query.AccountId.Value ||
                cf.DestinationAccountId == query.AccountId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            var categoryIdStr = query.CategoryId.Value.ToString();
            filteredCashFlows = filteredCashFlows.Where(cf => cf.CategoryId == categoryIdStr);
        }

        if (query.StartDate.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Date >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Date <= query.EndDate.Value);
        }

        // Appliquer la recherche sur label et reference
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchLower = query.Search.ToLower();
            filteredCashFlows = filteredCashFlows.Where(cf =>
                (cf.Label != null && cf.Label.ToLower().Contains(searchLower)) ||
                (cf.Reference != null && cf.Reference.ToLower().Contains(searchLower)));
        }

        // Convertir en DTOs d'export
        var exportDtos = filteredCashFlows
            .OrderByDescending(cf => cf.Date)
            .Select(cf => MapToExportDto(cf, categoryDict, accountDict))
            .ToList();

        // Valider les colonnes demandees
        string[]? validColumns = null;
        if (query.Columns != null && query.Columns.Length > 0)
        {
            validColumns = query.Columns
                .Where(ExportColumns.IsValidColumn)
                .ToArray();

            if (validColumns.Length == 0)
            {
                validColumns = null; // Utiliser toutes les colonnes si aucune n'est valide
            }
        }

        // Generer le fichier selon le format demande
        var exportDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        return query.Format switch
        {
            ExportFormat.Excel => new ExportCashFlowsResponse(
                FileStream: await excelExportService.ExportAsync(exportDtos, validColumns),
                FileName: $"flux-tresorerie-{exportDate}.xlsx",
                ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            ),
            _ => new ExportCashFlowsResponse(
                FileStream: await csvExportService.ExportAsync(exportDtos, validColumns),
                FileName: $"flux-tresorerie-{exportDate}.csv",
                ContentType: "text/csv"
            )
        };
    }

    private static CashFlowExportDto MapToExportDto(
        CashFlow cf,
        Dictionary<string, string> categoryDict,
        Dictionary<Guid, string> accountDict)
    {
        return new CashFlowExportDto(
            Reference: cf.Reference ?? string.Empty,
            Type: MapCashFlowType(cf.Type),
            Statut: MapCashFlowStatus(cf.Status),
            Categorie: categoryDict.TryGetValue(cf.CategoryId, out var catName) ? catName : "Inconnu",
            Libelle: cf.Label,
            Description: cf.Description,
            Montant: cf.Amount,
            Taxes: cf.TaxAmount,
            TauxTVA: cf.TaxRate,
            Devise: cf.Currency,
            Compte: accountDict.TryGetValue(cf.AccountId, out var accName) ? accName : "Inconnu",
            CompteDestination: cf.DestinationAccountId.HasValue &&
                               accountDict.TryGetValue(cf.DestinationAccountId.Value, out var destAccName)
                ? destAccName
                : null,
            ModePaiement: cf.PaymentMethod,
            Date: cf.Date,
            TypeDeTiers: cf.ThirdPartyType.HasValue ? MapThirdPartyType(cf.ThirdPartyType.Value) : null,
            NomDuTiers: cf.ThirdPartyName,
            IdTiers: cf.ThirdPartyId,
            CreeLe: cf.CreatedAt,
            CreePar: cf.CreatedBy ?? string.Empty,
            SoumisLe: cf.SubmittedAt,
            SoumisPar: cf.SubmittedBy,
            ValideLe: cf.ValidatedAt,
            ValidePar: cf.ValidatedBy,
            RaisonDeRejet: cf.RejectionReason,
            Rapproche: cf.IsReconciled ? "Oui" : "Non",
            ReferenceBancaire: cf.BankStatementReference
        );
    }

    private static string MapCashFlowType(CashFlowType type)
    {
        return type switch
        {
            CashFlowType.INCOME => "Recette",
            CashFlowType.EXPENSE => "Depense",
            CashFlowType.TRANSFER => "Virement",
            _ => type.ToString()
        };
    }

    private static string MapCashFlowStatus(CashFlowStatus status)
    {
        return status switch
        {
            CashFlowStatus.DRAFT => "Brouillon",
            CashFlowStatus.PENDING => "En attente",
            CashFlowStatus.APPROVED => "Approuve",
            CashFlowStatus.REJECTED => "Rejete",
            CashFlowStatus.CANCELLED => "Annule",
            _ => status.ToString()
        };
    }

    private static string MapThirdPartyType(ThirdPartyType thirdPartyType)
    {
        return thirdPartyType switch
        {
            ThirdPartyType.CUSTOMER => "Client",
            ThirdPartyType.SUPPLIER => "Fournisseur",
            ThirdPartyType.EMPLOYEE => "Employe",
            ThirdPartyType.OTHER => "Autre",
            _ => thirdPartyType.ToString()
        };
    }
}
