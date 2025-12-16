using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.CreateCashFlowFromSale;

/// <summary>
/// Handler pour creer automatiquement un flux de tresorerie (CashFlow)
/// quand une vente est validee.
///
/// Criteres d'acceptation:
/// - Type = INCOME
/// - Status = APPROVED (vente validee = encaissement confirme)
/// - Le compte est credite automatiquement
/// - RelatedType = "SALE", RelatedId = saleId
/// - IsSystemGenerated = true
/// - Label = "Vente #[reference]"
/// - ThirdPartyType = CUSTOMER
/// </summary>
public class CreateCashFlowFromSaleHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCashFlowFromSaleCommand, CreateCashFlowFromSaleResult>
{
    public async Task<CreateCashFlowFromSaleResult> Handle(
        CreateCashFlowFromSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        // Valider le format du CategoryId
        if (!Guid.TryParse(command.CategoryId, out var categoryGuid))
        {
            throw new BadRequestException($"L'identifiant de categorie '{command.CategoryId}' n'est pas un GUID valide");
        }

        // Valider que la categorie existe et est de type INCOME
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

        if (category.Type != CategoryType.INCOME)
        {
            throw new BadRequestException("La categorie doit etre de type INCOME pour un encaissement de vente");
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

        // Verifier si un flux existe deja pour cette vente (eviter les doublons)
        var existingCashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.RelatedType == "SALE"
                  && cf.RelatedId == command.SaleId.ToString()
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        if (existingCashFlows.Any())
        {
            throw new BadRequestException($"Un flux de tresorerie existe deja pour la vente #{command.SaleReference}");
        }

        // Generer une reference unique pour le flux
        var reference = GenerateSaleReference();

        // Creer le flux de tresorerie avec les criteres requis
        var cashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = command.ApplicationId,
            BoutiqueId = command.BoutiqueId,
            Reference = reference,
            Type = CashFlowType.INCOME,
            Status = CashFlowStatus.APPROVED,
            CategoryId = command.CategoryId,
            Label = $"Vente #{command.SaleReference}",
            Description = $"Encaissement automatique de la vente #{command.SaleReference}",
            Amount = command.Amount,
            TaxAmount = 0,
            TaxRate = 0,
            Currency = account.Currency,
            AccountId = command.AccountId,
            DestinationAccountId = null,
            PaymentMethod = command.PaymentMethod,
            Date = command.SaleDate,
            ThirdPartyType = ThirdPartyType.CUSTOMER,
            ThirdPartyName = command.CustomerName,
            ThirdPartyId = command.CustomerId,
            AttachmentUrl = null,
            IsReconciled = false,
            IsRecurring = false,
            IsSystemGenerated = true,
            AutoApproved = true,
            RelatedType = "SALE",
            RelatedId = command.SaleId.ToString(),
            ValidatedAt = DateTime.UtcNow,
            ValidatedBy = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            UpdatedBy = "SYSTEM"
        };

        // Crediter le compte automatiquement (car Status = APPROVED)
        account.CurrentBalance += command.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = "SYSTEM";

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.CREATED,
            OldStatus = null,
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = $"Flux cree automatiquement depuis la vente #{command.SaleReference}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "SYSTEM"
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

        return new CreateCashFlowFromSaleResult(cashFlowDto, account.CurrentBalance);
    }

    private static string GenerateSaleReference()
    {
        // Format: SAL-YYYYMMDD-XXXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..5].ToUpper();
        return $"SAL-{datePart}-{randomPart}";
    }
}
