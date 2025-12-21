using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromPurchase;

/// <summary>
/// Handler pour creer automatiquement un flux de tresorerie (CashFlow)
/// quand un achat est valide.
///
/// Criteres d'acceptation:
/// - Type = EXPENSE
/// - Status = APPROVED (achat valide = decaissement confirme)
/// - Le compte est debite automatiquement
/// - RelatedType = "PURCHASE", RelatedId = purchaseId
/// - IsSystemGenerated = true
/// - Label = "Achat #[reference]"
/// - ThirdPartyType = SUPPLIER
/// </summary>
public class CreateCashFlowFromPurchaseHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCashFlowFromPurchaseCommand, CreateCashFlowFromPurchaseResult>
{
    public async Task<CreateCashFlowFromPurchaseResult> Handle(
        CreateCashFlowFromPurchaseCommand command,
        CancellationToken cancellationToken = default)
    {
        // Valider le format du CategoryId
        if (!Guid.TryParse(command.CategoryId, out var categoryGuid))
        {
            throw new BadRequestException($"L'identifiant de categorie '{command.CategoryId}' n'est pas un GUID valide");
        }

        // Valider que la categorie existe et est de type EXPENSE
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

        if (category.Type != CategoryType.EXPENSE)
        {
            throw new BadRequestException("La categorie doit etre de type EXPENSE pour un decaissement d'achat");
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
            throw new NotFoundException("Compte non trouve ou inactif");
        }

        // Verifier si un flux existe deja pour cet achat (eviter les doublons)
        var existingCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.RelatedType == "PURCHASE"
                  && cf.RelatedId == command.PurchaseId.ToString()
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        if (existingCashFlows.Any())
        {
            throw new BadRequestException($"Un flux de tresorerie existe deja pour l'achat #{command.PurchaseReference}");
        }

        // Generer une reference unique pour le flux
        var reference = GeneratePurchaseReference();

        // Creer le flux de tresorerie avec les criteres requis
        // Note: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy sont gérés automatiquement par IAuditableEntity via UnitOfWork
        var cashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Reference = reference,
            Type = CashFlowType.EXPENSE,
            Status = CashFlowStatus.APPROVED,
            CategoryId = command.CategoryId,
            Label = $"Achat #{command.PurchaseReference}",
            Description = $"Decaissement automatique de l'achat #{command.PurchaseReference}",
            Amount = command.Amount,
            TaxAmount = 0,
            TaxRate = 0,
            Currency = account.Currency,
            AccountId = command.AccountId,
            DestinationAccountId = null,
            PaymentMethod = command.PaymentMethod,
            Date = command.PurchaseDate.ToUtc(),
            ThirdPartyType = ThirdPartyType.SUPPLIER,
            ThirdPartyName = command.SupplierName,
            ThirdPartyId = command.SupplierId,
            AttachmentUrl = null,
            IsReconciled = false,
            IsRecurring = false,
            IsSystemGenerated = true,
            AutoApproved = true,
            RelatedType = "PURCHASE",
            RelatedId = command.PurchaseId.ToString(),
            ValidatedAt = DateTime.UtcNow,
            ValidatedBy = "SYSTEM"
        };

        // Debiter le compte automatiquement (car Status = APPROVED et Type = EXPENSE)
        // Note: UpdatedAt, UpdatedBy sont gérés automatiquement par IAuditableEntity via UnitOfWork
        account.CurrentBalance -= command.Amount;

        // Creer une entree dans l'historique
        // Note: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy sont gérés automatiquement par IAuditableEntity via UnitOfWork
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.CREATED,
            OldStatus = null,
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = $"Flux cree automatiquement depuis l'achat #{command.PurchaseReference}"
        };

        // Sauvegarder les modifications
        await cashFlowRepository.AddDataAsync(cashFlow, cancellationToken);
        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        accountRepository.UpdateData(account);
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

        return new CreateCashFlowFromPurchaseResult(cashFlowDto, account.CurrentBalance);
    }

    private static string GeneratePurchaseReference()
    {
        // Format: PUR-YYYYMMDD-XXXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..5].ToUpper();
        return $"PUR-{datePart}-{randomPart}";
    }
}
