using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Repositories;
using Mapster;
using TresorerieService.Application.Features.Accounts.DTOs;
using TresorerieService.Domain.Entities;

namespace TresorerieService.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountHandler(
        IGenericRepository<Account> accountRepository,
        IUnitOfWork unitOfWork
    )
    : ICommandHandler<CreateAccountCommand, CreateAccountResult>
{
    public async Task<CreateAccountResult> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        // Si le compte est marqué comme défaut, retirer le défaut des autres comptes
        if (command.IsDefault)
        {
            var existingDefaults = await accountRepository.GetByConditionAsync(
                a => a.ApplicationId == command.ApplicationId
                     && a.BoutiqueId == command.BoutiqueId
                     && a.IsDefault,
                cancellationToken);

            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
                accountRepository.UpdateData(existingDefault);
            }
        }

        // Créer le nouveau compte
        var account = new Account
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Name = command.Name,
            Description = command.Description,
            Type = command.Type,
            InitialBalance = command.InitialBalance,
            CurrentBalance = command.InitialBalance,
            AlertThreshold = command.AlertThreshold,
            OverdraftLimit = command.OverdraftLimit,
            AccountNumber = command.AccountNumber,
            BankName = command.BankName,
            IsDefault = command.IsDefault,
            IsActive = true,
            Currency = "FCFA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        await accountRepository.AddDataAsync(account, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        var accountDto = account.Adapt<AccountDTO>();

        return new CreateAccountResult(accountDto);
    }
}
