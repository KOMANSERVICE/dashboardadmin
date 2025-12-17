namespace TresorerieService.Application.Features.CashFlows.Queries.GetUnreconciledCashFlows;

public class GetUnreconciledCashFlowsHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository
) : IQueryHandler<GetUnreconciledCashFlowsQuery, GetUnreconciledCashFlowsResponse>
{
    public async Task<GetUnreconciledCashFlowsResponse> Handle(
        GetUnreconciledCashFlowsQuery query,
        CancellationToken cancellationToken = default)
    {
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

        // Recuperer les flux non reconcilies (Status = APPROVED et IsReconciled = false)
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId
                  && cf.BoutiqueId == query.BoutiqueId
                  && cf.Status == CashFlowStatus.APPROVED
                  && cf.IsReconciled == false,
            cancellationToken);

        // Convertir en enumerable pour appliquer les filtres optionnels
        IEnumerable<CashFlow> filteredCashFlows = cashFlows;

        // Appliquer le filtre par compte si specifie
        if (query.AccountId.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf =>
                cf.AccountId == query.AccountId.Value ||
                cf.DestinationAccountId == query.AccountId.Value);
        }

        // Appliquer le filtre par periode si specifie
        if (query.StartDate.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Date >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Date <= query.EndDate.Value);
        }

        // Convertir en liste et trier par date (les plus anciens en premier)
        var sortedCashFlows = filteredCashFlows
            .OrderBy(cf => cf.Date)
            .ToList();

        // Compter le nombre de flux non reconcilies
        var unreconciledCount = sortedCashFlows.Count;

        // Calculer le montant total non reconcilie
        var totalUnreconciledAmount = sortedCashFlows.Sum(cf => cf.Amount);

        // Mapper vers les DTOs
        var cashFlowDtos = sortedCashFlows
            .Select(cf => new UnreconciledCashFlowDto(
                Id: cf.Id,
                Reference: cf.Reference,
                Type: cf.Type,
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
                ThirdPartyType: cf.ThirdPartyType,
                ThirdPartyName: cf.ThirdPartyName,
                ValidatedAt: cf.ValidatedAt,
                ValidatedBy: cf.ValidatedBy
            ))
            .ToList();

        return new GetUnreconciledCashFlowsResponse(
            CashFlows: cashFlowDtos,
            UnreconciledCount: unreconciledCount,
            TotalUnreconciledAmount: totalUnreconciledAmount
        );
    }
}
