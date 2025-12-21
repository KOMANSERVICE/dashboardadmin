using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.UpdateCashFlow;

public class UpdateCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateCashFlowCommand, UpdateCashFlowResult>
{
    public async Task<UpdateCashFlowResult> Handle(
        UpdateCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le flux de tresorerie
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            c => c.Id == command.Id
                 && c.ApplicationId == command.ApplicationId
                 && c.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlow = cashFlows.FirstOrDefault();
        if (cashFlow == null)
        {
            throw new NotFoundException("Flux non trouve");
        }

        // Verifier que le flux est en brouillon
        if (cashFlow.Status != CashFlowStatus.DRAFT)
        {
            throw new BadRequestException("Seuls les flux en brouillon peuvent etre modifies");
        }

        // Verifier que l'utilisateur est l'auteur du flux
        if (cashFlow.CreatedBy != command.UserId)
        {
            throw new BadRequestException("Vous ne pouvez modifier que vos propres flux");
        }

        var data = command.Data;

        // Valider et mettre a jour la categorie si fournie
        Category? category = null;
        if (!string.IsNullOrEmpty(data.CategoryId))
        {
            if (!Guid.TryParse(data.CategoryId, out var categoryGuid))
            {
                throw new BadRequestException($"L'identifiant de categorie '{data.CategoryId}' n'est pas un GUID valide");
            }

            var categories = await categoryRepository.GetByConditionAsync(
                c => c.Id == categoryGuid
                     && c.ApplicationId == command.ApplicationId
                     && c.IsActive,
                cancellationToken);

            category = categories.FirstOrDefault();
            if (category == null)
            {
                throw new NotFoundException($"La categorie avec l'ID '{data.CategoryId}' n'existe pas ou n'est pas active");
            }

            // Valider que le type de categorie correspond au type de flux
            var expectedCategoryType = cashFlow.Type == CashFlowType.INCOME ? CategoryType.INCOME : CategoryType.EXPENSE;
            if (cashFlow.Type != CashFlowType.TRANSFER && category.Type != expectedCategoryType)
            {
                var typeLabel = cashFlow.Type == CashFlowType.INCOME ? "INCOME" : "EXPENSE";
                throw new BadRequestException($"La categorie doit etre de type {typeLabel}");
            }

            cashFlow.CategoryId = data.CategoryId;
        }

        // Valider et mettre a jour le compte si fourni
        Account? account = null;
        if (data.AccountId.HasValue)
        {
            var accounts = await accountRepository.GetByConditionAsync(
                a => a.Id == data.AccountId.Value
                     && a.ApplicationId == command.ApplicationId
                     && a.BoutiqueId == command.BoutiqueId
                     && a.IsActive,
                cancellationToken);

            account = accounts.FirstOrDefault();
            if (account == null)
            {
                throw new NotFoundException("Compte non trouve");
            }

            cashFlow.AccountId = data.AccountId.Value;
        }

        // Valider et mettre a jour le compte de destination si fourni (pour les transferts)
        if (data.DestinationAccountId.HasValue)
        {
            if (cashFlow.Type != CashFlowType.TRANSFER)
            {
                throw new BadRequestException("Le compte de destination ne peut etre defini que pour les transferts");
            }

            var destinationAccounts = await accountRepository.GetByConditionAsync(
                a => a.Id == data.DestinationAccountId.Value
                     && a.ApplicationId == command.ApplicationId
                     && a.BoutiqueId == command.BoutiqueId
                     && a.IsActive,
                cancellationToken);

            var destinationAccount = destinationAccounts.FirstOrDefault();
            if (destinationAccount == null)
            {
                throw new NotFoundException("Compte de destination non trouve");
            }

            cashFlow.DestinationAccountId = data.DestinationAccountId.Value;
        }

        // Mettre a jour les champs modifiables
        if (!string.IsNullOrEmpty(data.Label))
        {
            cashFlow.Label = data.Label;
        }

        if (data.Description != null)
        {
            cashFlow.Description = data.Description;
        }

        if (data.Amount.HasValue)
        {
            cashFlow.Amount = data.Amount.Value;
        }

        if (data.TaxAmount.HasValue)
        {
            cashFlow.TaxAmount = data.TaxAmount.Value;
        }

        if (data.TaxRate.HasValue)
        {
            cashFlow.TaxRate = data.TaxRate.Value;
        }

        if (!string.IsNullOrEmpty(data.Currency))
        {
            cashFlow.Currency = data.Currency;
        }

        if (!string.IsNullOrEmpty(data.PaymentMethod))
        {
            cashFlow.PaymentMethod = data.PaymentMethod;
        }

        if (data.Date.HasValue)
        {
            cashFlow.Date = data.Date.Value.ToUtc();
        }

        if (data.ThirdPartyType.HasValue)
        {
            cashFlow.ThirdPartyType = data.ThirdPartyType.Value;
        }

        if (data.ThirdPartyName != null)
        {
            cashFlow.ThirdPartyName = data.ThirdPartyName;
        }

        if (data.ThirdPartyId != null)
        {
            cashFlow.ThirdPartyId = data.ThirdPartyId;
        }

        if (data.AttachmentUrl != null)
        {
            cashFlow.AttachmentUrl = data.AttachmentUrl;
        }

        // Mettre a jour les champs de tracking
        cashFlow.UpdatedAt = DateTime.UtcNow;
        cashFlow.UpdatedBy = command.UserId;

        cashFlowRepository.UpdateData(cashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Charger les entites liees pour le DTO de reponse
        if (category == null)
        {
            if (Guid.TryParse(cashFlow.CategoryId, out var categoryGuid))
            {
                var categories = await categoryRepository.GetByConditionAsync(
                    c => c.Id == categoryGuid,
                    cancellationToken);
                category = categories.FirstOrDefault();
            }
        }

        if (account == null)
        {
            var accounts = await accountRepository.GetByConditionAsync(
                a => a.Id == cashFlow.AccountId,
                cancellationToken);
            account = accounts.FirstOrDefault();
        }

        Account? destAccount = null;
        if (cashFlow.DestinationAccountId.HasValue)
        {
            var destAccounts = await accountRepository.GetByConditionAsync(
                a => a.Id == cashFlow.DestinationAccountId.Value,
                cancellationToken);
            destAccount = destAccounts.FirstOrDefault();
        }

        // Construire le DTO de reponse
        var cashFlowDto = new CashFlowDTO(
            Id: cashFlow.Id,
            ApplicationId: cashFlow.ApplicationId,
            BoutiqueId: cashFlow.BoutiqueId,
            Reference: cashFlow.Reference,
            Type: cashFlow.Type,
            Status: cashFlow.Status,
            CategoryId: cashFlow.CategoryId,
            CategoryName: category?.Name ?? "",
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            Amount: cashFlow.Amount,
            TaxAmount: cashFlow.TaxAmount,
            TaxRate: cashFlow.TaxRate,
            Currency: cashFlow.Currency,
            AccountId: cashFlow.AccountId,
            AccountName: account?.Name ?? "",
            DestinationAccountId: cashFlow.DestinationAccountId,
            DestinationAccountName: destAccount?.Name,
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

        return new UpdateCashFlowResult(cashFlowDto);
    }
}
