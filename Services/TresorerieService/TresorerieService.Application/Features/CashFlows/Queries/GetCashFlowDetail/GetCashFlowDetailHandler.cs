using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Queries.GetCashFlowDetail;

public class GetCashFlowDetailHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository
) : IQueryHandler<GetCashFlowDetailQuery, GetCashFlowDetailResponse>
{
    public async Task<GetCashFlowDetailResponse> Handle(
        GetCashFlowDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le flux par ID
        var cashFlow = await cashFlowRepository.GetByIdAsync(query.CashFlowId, cancellationToken);

        if (cashFlow == null)
        {
            throw new NotFoundException("CashFlow", query.CashFlowId);
        }

        // Verifier que le flux appartient a la bonne application et boutique
        if (cashFlow.ApplicationId != query.ApplicationId || cashFlow.BoutiqueId != query.BoutiqueId)
        {
            throw new NotFoundException("CashFlow", query.CashFlowId);
        }

        // Verifier les droits d'acces : un employe ne peut voir que ses propres flux
        if (!query.IsManager && cashFlow.CreatedBy != query.UserId)
        {
            throw new NotFoundException("CashFlow", query.CashFlowId);
        }

        // Recuperer le nom de la categorie
        string categoryName = "Inconnue";
        if (!string.IsNullOrEmpty(cashFlow.CategoryId))
        {
            if (Guid.TryParse(cashFlow.CategoryId, out var categoryGuid))
            {
                var category = await categoryRepository.GetByIdAsync(categoryGuid, cancellationToken);
                if (category != null)
                {
                    categoryName = category.Name;
                }
            }
        }

        // Recuperer le nom du compte source
        string accountName = "Inconnu";
        var account = await accountRepository.GetByIdAsync(cashFlow.AccountId, cancellationToken);
        if (account != null)
        {
            accountName = account.Name;
        }

        // Recuperer le nom du compte destination si TRANSFER
        string? destinationAccountName = null;
        if (cashFlow.DestinationAccountId.HasValue)
        {
            var destinationAccount = await accountRepository.GetByIdAsync(
                cashFlow.DestinationAccountId.Value,
                cancellationToken);
            if (destinationAccount != null)
            {
                destinationAccountName = destinationAccount.Name;
            }
        }

        // Mapper vers le DTO de detail
        var dto = new CashFlowDetailDto(
            Id: cashFlow.Id,
            ApplicationId: cashFlow.ApplicationId,
            BoutiqueId: cashFlow.BoutiqueId,
            Reference: cashFlow.Reference,
            Type: cashFlow.Type,
            Status: cashFlow.Status,
            CategoryId: cashFlow.CategoryId,
            CategoryName: categoryName,
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            Amount: cashFlow.Amount,
            TaxAmount: cashFlow.TaxAmount,
            TaxRate: cashFlow.TaxRate,
            Currency: cashFlow.Currency,
            AccountId: cashFlow.AccountId,
            AccountName: accountName,
            DestinationAccountId: cashFlow.DestinationAccountId,
            DestinationAccountName: destinationAccountName,
            PaymentMethod: cashFlow.PaymentMethod,
            Date: cashFlow.Date,
            ThirdPartyType: cashFlow.ThirdPartyType,
            ThirdPartyName: cashFlow.ThirdPartyName,
            ThirdPartyId: cashFlow.ThirdPartyId,
            AttachmentUrl: cashFlow.AttachmentUrl,
            RelatedType: cashFlow.RelatedType,
            RelatedId: cashFlow.RelatedId,
            IsRecurring: cashFlow.IsRecurring,
            RecurringCashFlowId: cashFlow.RecurringCashFlowId,
            IsSystemGenerated: cashFlow.IsSystemGenerated,
            AutoApproved: cashFlow.AutoApproved,
            IsReconciled: cashFlow.IsReconciled,
            ReconciledAt: cashFlow.ReconciledAt,
            ReconciledBy: cashFlow.ReconciledBy,
            BankStatementReference: cashFlow.BankStatementReference,
            CreatedAt: cashFlow.CreatedAt,
            CreatedBy: cashFlow.CreatedBy,
            SubmittedAt: cashFlow.SubmittedAt,
            SubmittedBy: cashFlow.SubmittedBy,
            ValidatedAt: cashFlow.ValidatedAt,
            ValidatedBy: cashFlow.ValidatedBy,
            RejectionReason: cashFlow.RejectionReason,
            // Budget non implemente - toujours null
            BudgetId: null,
            BudgetName: null,
            BudgetImpact: null
        );

        return new GetCashFlowDetailResponse(dto);
    }
}
