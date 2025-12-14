using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Entities;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccountBalance;

public class GetAccountBalanceHandler(
    IGenericRepository<Account> accountRepository,
    IGenericRepository<CashFlow> cashFlowRepository)
    : IQueryHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    public async Task<AccountBalanceDto> Handle(
        GetAccountBalanceQuery query,
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

        // Calculer les dates pour les filtres
        var now = DateTime.UtcNow;
        var startOfDay = now.Date;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Recuperer les mouvements APPROVED pour la periode du mois (inclut aujourd'hui)
        var monthCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.AccountId == query.AccountId &&
                  cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= startOfMonth,
            cancellationToken);

        // Recuperer les transferts entrants APPROVED pour la periode du mois
        var monthIncomingTransfers = await cashFlowRepository.GetByConditionAsync(
            cf => cf.DestinationAccountId == query.AccountId &&
                  cf.ApplicationId == query.ApplicationId &&
                  cf.BoutiqueId == query.BoutiqueId &&
                  cf.Type == CashFlowType.TRANSFER &&
                  cf.Status == CashFlowStatus.APPROVED &&
                  cf.Date >= startOfMonth,
            cancellationToken);

        // Calculer la variation depuis le debut du mois
        var variationThisMonth = CalculateVariation(monthCashFlows, monthIncomingTransfers);

        // Calculer la variation depuis le debut de la journee
        // Filtrer les mouvements d'aujourd'hui depuis la liste du mois
        var todayCashFlows = monthCashFlows.Where(cf => cf.Date >= startOfDay).ToList();
        var todayIncomingTransfers = monthIncomingTransfers.Where(cf => cf.Date >= startOfDay).ToList();
        var variationToday = CalculateVariation(todayCashFlows, todayIncomingTransfers);

        // Determiner si l'alerte est declenchee
        var isAlertTriggered = account.AlertThreshold.HasValue &&
                               account.CurrentBalance < account.AlertThreshold.Value;

        return new AccountBalanceDto(
            AccountId: account.Id,
            AccountName: account.Name,
            CurrentBalance: account.CurrentBalance,
            Currency: account.Currency,
            VariationToday: variationToday,
            VariationThisMonth: variationThisMonth,
            IsAlertTriggered: isAlertTriggered,
            AlertThreshold: account.AlertThreshold,
            CalculatedAt: now
        );
    }

    /// <summary>
    /// Calcule la variation nette des mouvements
    /// INCOME et transferts entrants sont positifs
    /// EXPENSE et transferts sortants sont negatifs
    /// </summary>
    private static decimal CalculateVariation(
        IEnumerable<CashFlow> sourceCashFlows,
        IEnumerable<CashFlow> incomingTransfers)
    {
        var sourceFlows = sourceCashFlows.ToList();

        // Entrees = INCOME (depuis source)
        var income = sourceFlows
            .Where(cf => cf.Type == CashFlowType.INCOME)
            .Sum(cf => cf.Amount);

        // Transferts entrants (ce compte est destination)
        var transfersIn = incomingTransfers.Sum(cf => cf.Amount);

        // Sorties = EXPENSE + TRANSFER sortants (depuis source)
        var expense = sourceFlows
            .Where(cf => cf.Type == CashFlowType.EXPENSE)
            .Sum(cf => cf.Amount);

        var transfersOut = sourceFlows
            .Where(cf => cf.Type == CashFlowType.TRANSFER)
            .Sum(cf => cf.Amount);

        // Variation = Entrees - Sorties
        return income + transfersIn - expense - transfersOut;
    }
}
