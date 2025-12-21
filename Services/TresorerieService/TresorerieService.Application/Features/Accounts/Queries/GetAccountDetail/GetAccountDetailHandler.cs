using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccountDetail;

public class GetAccountDetailHandler(
    IGenericRepository<Account> accountRepository,
    IGenericRepository<CashFlow> cashFlowRepository)
    : IQueryHandler<GetAccountDetailQuery, GetAccountDetailResponse>
{
    private const int MaxRecentMovements = 20;

    public async Task<GetAccountDetailResponse> Handle(
        GetAccountDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le compte
        var account = await accountRepository.GetByIdAsync(query.AccountId, cancellationToken);

        // Verifier que le compte existe et appartient a l'application/boutique
        if (account == null ||
            account.ApplicationId != query.ApplicationId ||
            account.BoutiqueId != query.BoutiqueId)
        {
            throw new NotFoundException("Account", query.AccountId);
        }

        // Recuperer tous les mouvements ou ce compte est la source
        var sourceCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.AccountId == query.AccountId &&
                  cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId,
            cancellationToken);

        // Recuperer les mouvements ou ce compte est la destination (transferts entrants)
        var destinationCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.DestinationAccountId == query.AccountId &&
                  cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Type == CashFlowType.TRANSFER,
            cancellationToken);

        // Combiner tous les mouvements
        var allCashFlows = sourceCashFlows.Concat(destinationCashFlows).ToList();

        // Calculer les totaux (uniquement sur les mouvements APPROVED)
        var approvedSourceCashFlows = sourceCashFlows
            .Where(cf => cf.Status == CashFlowStatus.APPROVED)
            .ToList();

        var approvedDestinationCashFlows = destinationCashFlows
            .Where(cf => cf.Status == CashFlowStatus.APPROVED)
            .ToList();

        // Entrees = INCOME + Transferts entrants (ou ce compte est destination)
        var totalIncome = approvedSourceCashFlows
            .Where(cf => cf.Type == CashFlowType.INCOME)
            .Sum(cf => cf.Amount)
            + approvedDestinationCashFlows.Sum(cf => cf.Amount);

        // Sorties = EXPENSE + Transferts sortants (ou ce compte est source)
        var totalExpense = approvedSourceCashFlows
            .Where(cf => cf.Type == CashFlowType.EXPENSE)
            .Sum(cf => cf.Amount)
            + approvedSourceCashFlows
                .Where(cf => cf.Type == CashFlowType.TRANSFER)
                .Sum(cf => cf.Amount);

        // Recuperer les 20 derniers mouvements (tous statuts)
        var recentMovements = allCashFlows
            .OrderByDescending(cf => cf.Date)
            .ThenByDescending(cf => cf.CreatedAt)
            .Take(MaxRecentMovements)
            .Select(cf => new CashFlowMovementDto(
                Id: cf.Id,
                Reference: cf.Reference,
                Type: cf.Type,
                Label: cf.Label,
                Description: cf.Description,
                Amount: cf.Amount,
                Currency: cf.Currency,
                Date: cf.Date,
                Status: cf.Status,
                ThirdPartyName: cf.ThirdPartyName
            ))
            .ToList();

        // Calculer l'evolution du solde si periode specifiee
        IReadOnlyList<BalanceEvolutionDto>? balanceEvolution = null;
        if (query.FromDate.HasValue && query.ToDate.HasValue)
        {
            balanceEvolution = CalculateBalanceEvolution(
                approvedSourceCashFlows,
                approvedDestinationCashFlows,
                account.InitialBalance,
                query.FromDate.Value,
                query.ToDate.Value);
        }

        // Calculer si le compte est en alerte
        var isInAlert = account.AlertThreshold.HasValue &&
                        account.CurrentBalance < account.AlertThreshold.Value;

        return new GetAccountDetailResponse(
            Id: account.Id,
            Name: account.Name,
            Description: account.Description,
            Type: account.Type,
            AccountNumber: account.AccountNumber,
            BankName: account.BankName,
            InitialBalance: account.InitialBalance,
            CurrentBalance: account.CurrentBalance,
            Currency: account.Currency,
            IsActive: account.IsActive,
            IsDefault: account.IsDefault,
            OverdraftLimit: account.OverdraftLimit,
            AlertThreshold: account.AlertThreshold,
            IsInAlert: isInAlert,
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            RecentMovements: recentMovements,
            BalanceEvolution: balanceEvolution,
            CreatedAt: account.CreatedAt,
            UpdatedAt: account.UpdatedAt
        );
    }

    private static List<BalanceEvolutionDto> CalculateBalanceEvolution(
        List<CashFlow> approvedSourceCashFlows,
        List<CashFlow> approvedDestinationCashFlows,
        decimal initialBalance,
        DateTime fromDate,
        DateTime toDate)
    {
        var result = new List<BalanceEvolutionDto>();

        // Calculer le solde de depart (tous les mouvements avant fromDate)
        var startingBalance = initialBalance;

        // Mouvements source avant la periode
        foreach (var cf in approvedSourceCashFlows.Where(cf => cf.Date < fromDate))
        {
            if (cf.Type == CashFlowType.INCOME)
                startingBalance += cf.Amount;
            else if (cf.Type == CashFlowType.EXPENSE || cf.Type == CashFlowType.TRANSFER)
                startingBalance -= cf.Amount;
        }

        // Transferts entrants avant la periode
        foreach (var cf in approvedDestinationCashFlows.Where(cf => cf.Date < fromDate))
        {
            startingBalance += cf.Amount;
        }

        // Filtrer les mouvements source sur la periode
        var periodSourceCashFlows = approvedSourceCashFlows
            .Where(cf => cf.Date >= fromDate && cf.Date <= toDate)
            .ToList();

        // Filtrer les transferts entrants sur la periode
        var periodDestinationCashFlows = approvedDestinationCashFlows
            .Where(cf => cf.Date >= fromDate && cf.Date <= toDate)
            .ToList();

        // Combiner et grouper par jour
        var allPeriodFlows = periodSourceCashFlows
            .Select(cf => new { CashFlow = cf, IsIncoming = false })
            .Concat(periodDestinationCashFlows.Select(cf => new { CashFlow = cf, IsIncoming = true }))
            .GroupBy(x => x.CashFlow.Date.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var currentBalance = startingBalance;

        foreach (var dayGroup in allPeriodFlows)
        {
            // Entrees = INCOME + Transferts entrants
            var dayIncome = dayGroup
                .Where(x => !x.IsIncoming && x.CashFlow.Type == CashFlowType.INCOME)
                .Sum(x => x.CashFlow.Amount)
                + dayGroup
                    .Where(x => x.IsIncoming)
                    .Sum(x => x.CashFlow.Amount);

            // Sorties = EXPENSE + Transferts sortants
            var dayExpense = dayGroup
                .Where(x => !x.IsIncoming && x.CashFlow.Type == CashFlowType.EXPENSE)
                .Sum(x => x.CashFlow.Amount)
                + dayGroup
                    .Where(x => !x.IsIncoming && x.CashFlow.Type == CashFlowType.TRANSFER)
                    .Sum(x => x.CashFlow.Amount);

            currentBalance += dayIncome - dayExpense;

            result.Add(new BalanceEvolutionDto(
                Date: dayGroup.Key,
                Balance: currentBalance,
                TotalIncome: dayIncome,
                TotalExpense: dayExpense
            ));
        }

        return result;
    }
}
