using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlow;

public class ReconcileCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ReconcileCashFlowCommand, ReconcileCashFlowResult>
{
    public async Task<ReconcileCashFlowResult> Handle(
        ReconcileCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verifier que l'utilisateur est manager ou admin
        var userRole = command.UserRole.ToLower();
        if (userRole != "manager" && userRole != "admin")
        {
            throw new BadRequestException("Acces refuse: seul un manager ou admin peut reconcilier un flux");
        }

        // Recuperer le flux de tresorerie
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.Id == command.CashFlowId
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlow = cashFlows.FirstOrDefault();
        if (cashFlow == null)
        {
            throw new NotFoundException($"Le flux de tresorerie avec l'ID '{command.CashFlowId}' n'existe pas");
        }

        // Verifier que le flux est en APPROVED (seul un flux approuve peut etre reconcilie)
        if (cashFlow.Status != CashFlowStatus.APPROVED)
        {
            throw new BadRequestException($"Seul un flux approuve (APPROVED) peut etre reconcilie. Statut actuel: {cashFlow.Status}");
        }

        // Verifier que le flux n'est pas deja reconcilie
        if (cashFlow.IsReconciled)
        {
            throw new BadRequestException("Ce flux est deja reconcilie");
        }

        // Mettre a jour les informations de reconciliation
        cashFlow.IsReconciled = true;
        cashFlow.ReconciledAt = DateTime.UtcNow;
        cashFlow.ReconciledBy = command.ReconciledBy;
        cashFlow.BankStatementReference = command.BankStatementReference;
        cashFlow.UpdatedAt = DateTime.UtcNow;
        cashFlow.UpdatedBy = command.ReconciledBy;

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.RECONCILED,
            OldStatus = cashFlow.Status.ToString(),
            NewStatus = cashFlow.Status.ToString(), // Le statut ne change pas, seul IsReconciled change
            Comment = string.IsNullOrEmpty(command.BankStatementReference)
                ? $"Flux reconcilie par {command.ReconciledBy}"
                : $"Flux reconcilie par {command.ReconciledBy} - Ref: {command.BankStatementReference}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.ReconciledBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = command.ReconciledBy
        };

        // Sauvegarder les modifications
        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        cashFlowRepository.UpdateData(cashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Construire le DTO de reponse complet
        var dto = await BuildCashFlowDetailDto(cashFlow, cancellationToken);

        return new ReconcileCashFlowResult(dto);
    }

    private async Task<CashFlowDetailDto> BuildCashFlowDetailDto(
        CashFlow cashFlow,
        CancellationToken cancellationToken)
    {
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

        return new CashFlowDetailDto(
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
            BudgetId: null,
            BudgetName: null,
            BudgetImpact: null
        );
    }
}
