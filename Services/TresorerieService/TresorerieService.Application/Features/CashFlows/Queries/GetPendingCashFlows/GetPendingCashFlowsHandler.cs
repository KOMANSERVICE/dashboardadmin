namespace TresorerieService.Application.Features.CashFlows.Queries.GetPendingCashFlows;

public class GetPendingCashFlowsHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository
) : IQueryHandler<GetPendingCashFlowsQuery, GetPendingCashFlowsResponse>
{
    public async Task<GetPendingCashFlowsResponse> Handle(
        GetPendingCashFlowsQuery query,
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

        // Recuperer les flux en attente (Status = PENDING)
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == query.ApplicationId
                  && cf.BoutiqueId == query.BoutiqueId
                  && cf.Status == CashFlowStatus.PENDING,
            cancellationToken);

        // Convertir en enumerable pour appliquer les filtres optionnels
        IEnumerable<CashFlow> filteredCashFlows = cashFlows;

        // Appliquer le filtre par type si specifie
        if (query.Type.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf => cf.Type == query.Type.Value);
        }

        // Appliquer le filtre par compte si specifie
        if (query.AccountId.HasValue)
        {
            filteredCashFlows = filteredCashFlows.Where(cf =>
                cf.AccountId == query.AccountId.Value ||
                cf.DestinationAccountId == query.AccountId.Value);
        }

        // Convertir en liste et trier par date de soumission (les plus anciens en premier)
        var sortedCashFlows = filteredCashFlows
            .OrderBy(cf => cf.SubmittedAt)
            .ToList();

        // Compter le nombre de flux en attente
        var pendingCount = sortedCashFlows.Count;

        // Mapper vers les DTOs
        var cashFlowDtos = sortedCashFlows
            .Select(cf => new PendingCashFlowDto(
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
                ThirdPartyName: cf.ThirdPartyName,
                SubmittedAt: cf.SubmittedAt,
                SubmittedBy: cf.SubmittedBy
            ))
            .ToList();

        return new GetPendingCashFlowsResponse(
            CashFlows: cashFlowDtos,
            PendingCount: pendingCount
        );
    }
}
