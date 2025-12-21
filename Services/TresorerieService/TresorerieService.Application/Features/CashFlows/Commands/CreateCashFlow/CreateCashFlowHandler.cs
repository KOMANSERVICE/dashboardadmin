using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlow;

public class CreateCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCashFlowCommand, CreateCashFlowResult>
{
    public async Task<CreateCashFlowResult> Handle(
        CreateCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Valider que le type est INCOME ou EXPENSE (pas TRANSFER)
        if (command.Type != CashFlowType.INCOME && command.Type != CashFlowType.EXPENSE)
        {
            throw new BadRequestException($"Le type de flux doit etre INCOME ou EXPENSE. Type fourni: {command.Type}");
        }

        // Valider le format du CategoryId
        if (!Guid.TryParse(command.CategoryId, out var categoryGuid))
        {
            throw new BadRequestException($"L'identifiant de categorie '{command.CategoryId}' n'est pas un GUID valide");
        }

        // Valider que la categorie existe et est du bon type
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id == categoryGuid
                 && c.ApplicationId == command.ApplicationId
                 && c.IsActive,
            cancellationToken);

        var category = categories.FirstOrDefault();
        if (category == null)
        {
            throw new NotFoundException($"La categorie avec l'ID '{command.CategoryId}' n'existe pas ou n'est pas active");
        }

        // Valider que le type de categorie correspond au type de flux
        var expectedCategoryType = command.Type == CashFlowType.INCOME ? CategoryType.INCOME : CategoryType.EXPENSE;
        if (category.Type != expectedCategoryType)
        {
            var typeLabel = command.Type == CashFlowType.INCOME ? "INCOME" : "EXPENSE";
            throw new BadRequestException($"La categorie doit etre de type {typeLabel}");
        }

        // Valider que le compte existe et appartient a la boutique
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == command.AccountId
                 && a.ApplicationId == command.ApplicationId
                 && a.BoutiqueId == command.BoutiqueId
                 && a.IsActive,
            cancellationToken);

        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            throw new NotFoundException("Compte non trouve");
        }

        // Generer une reference unique selon le type
        var reference = GenerateReference(command.Type);

        // Determiner le type de tiers selon le type de flux
        ThirdPartyType? thirdPartyType = null;
        if (!string.IsNullOrEmpty(command.ThirdPartyName))
        {
            thirdPartyType = command.Type == CashFlowType.INCOME ? ThirdPartyType.CUSTOMER : ThirdPartyType.SUPPLIER;
        }

        // Creer le flux de tresorerie
        var cashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Reference = reference,
            Type = command.Type,
            Status = CashFlowStatus.DRAFT,
            CategoryId = command.CategoryId,
            Label = command.Label,
            Description = command.Description,
            Amount = command.Amount,
            TaxAmount = 0,
            TaxRate = 0,
            Currency = account.Currency,
            AccountId = command.AccountId,
            DestinationAccountId = null,
            PaymentMethod = command.PaymentMethod,
            Date = command.Date.ToUtc(),
            ThirdPartyType = thirdPartyType,
            ThirdPartyName = command.ThirdPartyName,
            ThirdPartyId = null,
            AttachmentUrl = command.AttachmentUrl,
            IsReconciled = false,
            IsRecurring = false,
            IsSystemGenerated = false,
            AutoApproved = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = command.CreatedBy,
            UpdatedBy = command.CreatedBy
        };

        await cashFlowRepository.AddDataAsync(cashFlow, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Construire le DTO de reponse
        var cashFlowDto = new CashFlowDTO(
            Id: cashFlow.Id,
            ApplicationId: cashFlow.ApplicationId,
            BoutiqueId: cashFlow.BoutiqueId,
            Reference: cashFlow.Reference,
            Type: cashFlow.Type,
            Status: cashFlow.Status,
            CategoryId: cashFlow.CategoryId,
            CategoryName: category.Name,
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            Amount: cashFlow.Amount,
            TaxAmount: cashFlow.TaxAmount,
            TaxRate: cashFlow.TaxRate,
            Currency: cashFlow.Currency,
            AccountId: cashFlow.AccountId,
            AccountName: account.Name,
            DestinationAccountId: cashFlow.DestinationAccountId,
            DestinationAccountName: null,
            PaymentMethod: cashFlow.PaymentMethod,
            Date: cashFlow.Date,
            ThirdPartyType: cashFlow.ThirdPartyType,
            ThirdPartyName: cashFlow.ThirdPartyName,
            ThirdPartyId: cashFlow.ThirdPartyId,
            AttachmentUrl: cashFlow.AttachmentUrl,
            CreatedAt: cashFlow.CreatedAt,
            CreatedBy: cashFlow.CreatedBy,
            SubmittedAt: cashFlow.SubmittedAt,
            SubmittedBy: cashFlow.SubmittedBy,
            ValidatedAt: cashFlow.ValidatedAt,
            ValidatedBy: cashFlow.ValidatedBy,
            RejectionReason: cashFlow.RejectionReason
        );

        // Note: La fonctionnalite de budget n'existe pas pour les revenus (budgetId = null).
        // Pour les depenses, a implementer dans une prochaine User Story.
        string? budgetWarning = null;

        return new CreateCashFlowResult(cashFlowDto, budgetWarning);
    }

    private static string GenerateReference(CashFlowType type)
    {
        // Format: XXX-YYYYMMDD-XXXXX
        // INC pour INCOME, EXP pour EXPENSE
        var prefix = type == CashFlowType.INCOME ? "INC" : "EXP";
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..5].ToUpper();
        return $"{prefix}-{datePart}-{randomPart}";
    }
}
