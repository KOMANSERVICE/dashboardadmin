using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlows;

public class GetCashFlowsHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUserContextService userContextService
) : IQueryHandler<GetCashFlowsQuery, GetCashFlowsResponse>
{
    public async Task<GetCashFlowsResponse> Handle(
        GetCashFlowsQuery query,
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
        //TODO: a revoir pas tres bien gerer
        //IEnumerable<CashFlow> filteredCashFlows = query.IsManager
        //    ? cashFlows
        //    : cashFlows.Where(cf => cf.CreatedBy == email);

        IEnumerable<CashFlow> filteredCashFlows = cashFlows;
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

        // Convertir en liste pour les operations suivantes
        var cashFlowList = filteredCashFlows.ToList();

        // Compter le total avant pagination
        var totalCount = cashFlowList.Count;

        // Appliquer le tri
        cashFlowList = ApplySorting(cashFlowList, query.SortBy, query.SortOrder);

        // Appliquer la pagination
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize > 100 ? 100 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var pagedCashFlows = cashFlowList
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        // Mapper vers les DTOs
        var cashFlowDtos = pagedCashFlows
            .Select(cf => new CashFlowListDto(
                Id: cf.Id,
                Reference: cf.Reference,
                Type: cf.Type,
                Status: cf.Status,
                CategoryId: cf.CategoryId,
                CategoryName: categoryDict.TryGetValue(cf.CategoryId, out var catName) ? catName : "Inconnu",
                Label: cf.Label,
                Amount: cf.Amount,
                Currency: cf.Currency,
                AccountId: cf.AccountId,
                AccountName: accountDict.TryGetValue(cf.AccountId, out var accName) ? accName : "Inconnu",
                DestinationAccountId: cf.DestinationAccountId,
                DestinationAccountName: cf.DestinationAccountId.HasValue && accountDict.TryGetValue(cf.DestinationAccountId.Value, out var destAccName) ? destAccName : null,
                PaymentMethod: cf.PaymentMethod,
                Date: cf.Date,
                ThirdPartyName: cf.ThirdPartyName,
                CreatedAt: cf.CreatedAt,
                CreatedBy: cf.CreatedBy
            ))
            .ToList();

        // Calculer les informations de pagination
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var hasPrevious = page > 1;
        var hasNext = page < totalPages;

        return new GetCashFlowsResponse(
            CashFlows: cashFlowDtos,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages,
            HasPrevious: hasPrevious,
            HasNext: hasNext
        );
    }

    private static List<CashFlow> ApplySorting(List<CashFlow> cashFlows, string sortBy, string sortOrder)
    {
        var isDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "date" => isDescending
                ? cashFlows.OrderByDescending(cf => cf.Date).ToList()
                : cashFlows.OrderBy(cf => cf.Date).ToList(),

            "amount" => isDescending
                ? cashFlows.OrderByDescending(cf => cf.Amount).ToList()
                : cashFlows.OrderBy(cf => cf.Amount).ToList(),

            "createdat" => isDescending
                ? cashFlows.OrderByDescending(cf => cf.CreatedAt).ToList()
                : cashFlows.OrderBy(cf => cf.CreatedAt).ToList(),

            // Par defaut, trier par date descendante
            _ => cashFlows.OrderByDescending(cf => cf.Date).ToList()
        };
    }
}
