using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using IDR.Library.BuildingBlocks.Repositories;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountHandler(
        IGenericRepository<Account> accountRepository,
        IGenericRepository<CashFlow> cashFlowRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<UpdateAccountCommand, UpdateAccountResult>
{
    public async Task<UpdateAccountResult> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Vérifier l'existence du compte avec AccountId + BoutiqueId + ApplicationId
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == command.AccountId
                 && a.ApplicationId == command.ApplicationId
                 && a.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new NotFoundException("Compte non trouvé");
        }

        // 2. Si modification de InitialBalance demandée, vérifier qu'aucun flux n'existe
        if (command.Data.InitialBalance.HasValue &&
            command.Data.InitialBalance.Value != account.InitialBalance)
        {
            var hasCashFlows = await HasCashFlowsAsync(
                command.AccountId,
                command.ApplicationId,
                command.BoutiqueId,
                cancellationToken);

            if (hasCashFlows)
            {
                throw new BadRequestException(
                    "Impossible de modifier le solde initial car des flux sont déjà enregistrés sur ce compte");
            }

            // Calculer la différence pour mettre à jour le CurrentBalance
            var difference = command.Data.InitialBalance.Value - account.InitialBalance;
            account.InitialBalance = command.Data.InitialBalance.Value;
            account.CurrentBalance += difference;
        }

        // 3. Mettre à jour les champs modifiables
        account.Name = command.Data.Name;
        account.AlertThreshold = command.Data.AlertThreshold;
        account.OverdraftLimit = command.Data.OverdraftLimit;
        account.IsActive = command.Data.IsActive;

        // 4. Mettre à jour les informations d'audit
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = "system"; // TODO: Récupérer depuis le contexte utilisateur

        accountRepository.UpdateData(account);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new UpdateAccountResult(account.Id);
    }

    private async Task<bool> HasCashFlowsAsync(
        Guid accountId,
        string applicationId,
        string boutiqueId,
        CancellationToken cancellationToken)
    {
        // Vérifier les CashFlows où ce compte est source OU destination
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.ApplicationId == applicationId
                  && cf.BoutiqueId == boutiqueId
                  && (cf.AccountId == accountId || cf.DestinationAccountId == accountId),
            cancellationToken);

        return cashFlows.Any();
    }
}
