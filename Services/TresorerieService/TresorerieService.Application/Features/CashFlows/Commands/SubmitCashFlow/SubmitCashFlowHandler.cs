using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.SubmitCashFlow;

public class SubmitCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<SubmitCashFlowCommand, SubmitCashFlowResult>
{
    public async Task<SubmitCashFlowResult> Handle(
        SubmitCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
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

        // Verifier que le flux est en DRAFT
        if (cashFlow.Status != CashFlowStatus.DRAFT)
        {
            throw new BadRequestException($"Seul un flux en brouillon (DRAFT) peut etre soumis. Statut actuel: {cashFlow.Status}");
        }

        // Stocker l'ancien statut pour l'historique
        var oldStatus = cashFlow.Status.ToString();

        // Mettre a jour le statut et la date de soumission
        cashFlow.Status = CashFlowStatus.PENDING;
        cashFlow.SubmittedAt = DateTime.UtcNow;
        cashFlow.SubmittedBy = command.SubmittedBy;
        cashFlow.UpdatedAt = DateTime.UtcNow;
        cashFlow.UpdatedBy = command.SubmittedBy;

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.SUBMITTED,
            OldStatus = oldStatus,
            NewStatus = CashFlowStatus.PENDING.ToString(),
            Comment = "Flux soumis pour validation",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.SubmittedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = command.SubmittedBy
        };

        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        cashFlowRepository.UpdateData(cashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Recuperer les informations supplementaires pour le DTO
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id.ToString() == cashFlow.CategoryId,
            cancellationToken);
        var category = categories.FirstOrDefault();

        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == cashFlow.AccountId,
            cancellationToken);
        var account = accounts.FirstOrDefault();

        string? destinationAccountName = null;
        if (cashFlow.DestinationAccountId.HasValue)
        {
            var destAccounts = await accountRepository.GetByConditionAsync(
                a => a.Id == cashFlow.DestinationAccountId.Value,
                cancellationToken);
            var destAccount = destAccounts.FirstOrDefault();
            destinationAccountName = destAccount?.Name;
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
            CategoryName: category?.Name ?? "Categorie inconnue",
            Label: cashFlow.Label,
            Description: cashFlow.Description,
            Amount: cashFlow.Amount,
            TaxAmount: cashFlow.TaxAmount,
            TaxRate: cashFlow.TaxRate,
            Currency: cashFlow.Currency,
            AccountId: cashFlow.AccountId,
            AccountName: account?.Name ?? "Compte inconnu",
            DestinationAccountId: cashFlow.DestinationAccountId,
            DestinationAccountName: destinationAccountName,
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

        // Verification du budget pour les depenses
        // Note: Le champ BudgetId est commente dans l'entite CashFlow
        // Pour l'instant, la verification retourne toujours null car aucun budget n'est defini
        string? budgetWarning = null;

        if (cashFlow.Type == CashFlowType.EXPENSE)
        {
            // TODO: Implementer la verification de budget dans une prochaine US
            // Pour l'instant, on retourne null car le budget n'est pas encore implemente
            // Une fois l'entite Budget creee, on pourra verifier:
            // 1. Si un budget est defini pour cette categorie/periode
            // 2. Calculer le montant deja consomme
            // 3. Verifier si cette depense depasse le budget restant
            // 4. Retourner un avertissement si depassement
        }

        return new SubmitCashFlowResult(cashFlowDto, budgetWarning);
    }
}
