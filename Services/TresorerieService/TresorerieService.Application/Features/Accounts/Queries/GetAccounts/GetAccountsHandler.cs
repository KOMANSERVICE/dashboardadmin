using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Accounts.Queries.GetAccounts;

public class GetAccountsHandler(IGenericRepository<Account> accountRepository)
    : IQueryHandler<GetAccountsQuery, GetAccountsResponse>
{
    public async Task<GetAccountsResponse> Handle(
        GetAccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Récupérer les comptes selon les filtres
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.ApplicationId == query.ApplicationId
                 && a.BoutiqueId == query.BoutiqueId
                 && (query.IncludeInactive || a.IsActive)
                 && (!query.Type.HasValue || a.Type == query.Type.Value),
            cancellationToken);

        // Mapper vers les DTOs avec calcul de l'alerte
        var accountDtos = accounts
            .Select(a => new AccountListDto(
                Id: a.Id,
                Name: a.Name,
                Type: a.Type,
                CurrentBalance: a.CurrentBalance,
                Currency: a.Currency,
                IsActive: a.IsActive,
                IsDefault: a.IsDefault,
                AlertThreshold: a.AlertThreshold,
                IsInAlert: a.AlertThreshold.HasValue && a.CurrentBalance < a.AlertThreshold.Value
            ))
            .ToList();

        // Calculer le total disponible (somme des soldes des comptes actifs uniquement)
        var totalAvailable = accounts
            .Where(a => a.IsActive)
            .Sum(a => a.CurrentBalance);

        return new GetAccountsResponse(
            Accounts: accountDtos,
            TotalAvailable: totalAvailable,
            TotalCount: accountDtos.Count
        );
    }
}
