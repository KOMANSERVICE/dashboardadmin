namespace TresorerieService.Application.Features.CashFlows.Commands.CreateTransfer;

public class CreateTransferHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateTransferCommand, CreateTransferResult>
{
    public async Task<CreateTransferResult> Handle(
        CreateTransferCommand command,
        CancellationToken cancellationToken = default)
    {
        // Valider que les deux comptes sont differents
        if (command.AccountId == command.DestinationAccountId)
        {
            throw new BadRequestException("Le compte source et le compte destination doivent etre differents");
        }

        // Valider que le compte source existe et appartient a la boutique
        var sourceAccounts = await accountRepository.GetByConditionAsync(
            a => a.Id == command.AccountId
                 && a.ApplicationId == command.ApplicationId
                 && a.BoutiqueId == command.BoutiqueId
                 && a.IsActive,
            cancellationToken);

        var sourceAccount = sourceAccounts.FirstOrDefault();
        if (sourceAccount == null)
        {
            throw new NotFoundException("Compte source non trouve");
        }

        // Valider que le compte destination existe et appartient a la boutique
        var destinationAccounts = await accountRepository.GetByConditionAsync(
            a => a.Id == command.DestinationAccountId
                 && a.ApplicationId == command.ApplicationId
                 && a.BoutiqueId == command.BoutiqueId
                 && a.IsActive,
            cancellationToken);

        var destinationAccount = destinationAccounts.FirstOrDefault();
        if (destinationAccount == null)
        {
            throw new NotFoundException("Compte destination non trouve");
        }

        // Valider que le solde source est suffisant
        if (sourceAccount.CurrentBalance < command.Amount)
        {
            throw new BadRequestException("Solde insuffisant sur le compte source");
        }

        // Generer une reference unique pour le transfert
        var reference = GenerateTransferReference();

        // Creer le flux de tresorerie de type TRANSFER
        var cashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Reference = reference,
            Type = CashFlowType.TRANSFER,
            Status = CashFlowStatus.APPROVED, // Transfert approuve automatiquement
            CategoryId = "TRANSFER", // Categorie speciale pour les transferts
            Label = command.Label,
            Description = command.Description,
            Amount = command.Amount,
            TaxAmount = 0,
            TaxRate = 0,
            Currency = sourceAccount.Currency,
            AccountId = command.AccountId,
            DestinationAccountId = command.DestinationAccountId,
            PaymentMethod = "TRANSFER",
            Date = command.Date.ToUtc(),
            ThirdPartyType = null,
            ThirdPartyName = null,
            ThirdPartyId = null,
            AttachmentUrl = null,
            IsReconciled = false,
            IsRecurring = false,
            IsSystemGenerated = false,
            AutoApproved = true, // Transfert auto-approuve
            ValidatedAt = DateTime.UtcNow,
            ValidatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = command.CreatedBy,
            UpdatedBy = command.CreatedBy
        };

        // Mettre a jour les soldes des comptes
        sourceAccount.CurrentBalance -= command.Amount;
        sourceAccount.UpdatedAt = DateTime.UtcNow;
        sourceAccount.UpdatedBy = command.CreatedBy;

        destinationAccount.CurrentBalance += command.Amount;
        destinationAccount.UpdatedAt = DateTime.UtcNow;
        destinationAccount.UpdatedBy = command.CreatedBy;

        // Sauvegarder le flux et les comptes mis a jour
        await cashFlowRepository.AddDataAsync(cashFlow, cancellationToken);
        accountRepository.UpdateData(sourceAccount);
        accountRepository.UpdateData(destinationAccount);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Construire le DTO de reponse
        var transferDto = new TransferDto(
            Id: cashFlow.Id,
            Type: "TRANSFER",
            Status: "APPROVED",
            AccountId: cashFlow.AccountId,
            AccountName: sourceAccount.Name,
            DestinationAccountId: cashFlow.DestinationAccountId!.Value,
            DestinationAccountName: destinationAccount.Name,
            Amount: cashFlow.Amount,
            Date: cashFlow.Date,
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            SourceAccountBalance: sourceAccount.CurrentBalance,
            DestinationAccountBalance: destinationAccount.CurrentBalance,
            CreatedAt: cashFlow.CreatedAt,
            CreatedBy: cashFlow.CreatedBy
        );

        return new CreateTransferResult(transferDto);
    }

    private static string GenerateTransferReference()
    {
        // Format: TRF-YYYYMMDD-XXXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..5].ToUpper();
        return $"TRF-{datePart}-{randomPart}";
    }
}
