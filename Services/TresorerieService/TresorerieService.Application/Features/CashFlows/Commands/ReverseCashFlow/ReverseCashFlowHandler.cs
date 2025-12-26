using IDR.Library.BuildingBlocks.Contexts.Services;

namespace TresorerieService.Application.Features.CashFlows.Commands.ReverseCashFlow;

/// <summary>
/// Handler pour la contre-passation d'un CashFlow.
/// Cree un nouveau CashFlow avec le type inverse pour annuler l'effet du flux original.
/// </summary>
public class ReverseCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Account> accountRepository,
    IGenericRepository<Budget> budgetRepository,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService
) : ICommandHandler<ReverseCashFlowCommand, ReverseCashFlowResult>
{
    public async Task<ReverseCashFlowResult> Handle(
        ReverseCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = userContextService.GetEmail();

        // Recuperer le CashFlow original
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => cf.Id == command.CashFlowId
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var originalCashFlow = cashFlows.FirstOrDefault();

        // Validation: CashFlow existe
        if (originalCashFlow == null)
        {
            throw new NotFoundException($"Le CashFlow avec l'ID '{command.CashFlowId}' n'existe pas");
        }

        // Validation: CashFlow n'est pas deja contre-passe
        if (originalCashFlow.IsReversed)
        {
            throw new BadRequestException("Ce CashFlow a deja ete contre-passe");
        }

        // Validation: CashFlow n'est pas une contre-passation
        if (originalCashFlow.IsReversal)
        {
            throw new BadRequestException("Impossible de contre-passer une contre-passation");
        }

        // Validation: Seul un flux APPROVED peut etre contre-passe
        if (originalCashFlow.Status != CashFlowStatus.APPROVED)
        {
            throw new BadRequestException($"Seul un flux approuve (APPROVED) peut etre contre-passe. Statut actuel: {originalCashFlow.Status}");
        }

        // Determiner le type inverse
        var reversedType = originalCashFlow.Type switch
        {
            CashFlowType.INCOME => CashFlowType.EXPENSE,
            CashFlowType.EXPENSE => CashFlowType.INCOME,
            CashFlowType.TRANSFER => throw new BadRequestException("Les transferts ne peuvent pas etre contre-passes"),
            _ => throw new BadRequestException($"Type de flux non supporte: {originalCashFlow.Type}")
        };

        // Creer le CashFlow de contre-passation
        var reversalCashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = originalCashFlow.ApplicationId,
            BoutiqueId = originalCashFlow.BoutiqueId,
            Reference = $"REV-{originalCashFlow.Reference ?? originalCashFlow.Id.ToString()[..8]}",
            Type = reversedType,
            CategoryId = originalCashFlow.CategoryId,
            Label = $"[Contre-passation] {originalCashFlow.Label}",
            Description = $"Annulation du flux {originalCashFlow.Reference ?? originalCashFlow.Id.ToString()}: {command.Reason}",
            Amount = originalCashFlow.Amount,
            TaxAmount = originalCashFlow.TaxAmount,
            TaxRate = originalCashFlow.TaxRate,
            Currency = originalCashFlow.Currency,
            AccountId = originalCashFlow.AccountId,
            DestinationAccountId = null,
            PaymentMethod = originalCashFlow.PaymentMethod,
            Date = DateTime.UtcNow,
            ThirdPartyType = originalCashFlow.ThirdPartyType,
            ThirdPartyName = originalCashFlow.ThirdPartyName,
            ThirdPartyId = originalCashFlow.ThirdPartyId,
            Status = CashFlowStatus.APPROVED,
            ValidatedAt = DateTime.UtcNow,
            ValidatedBy = email,
            IsReconciled = false,
            IsRecurring = false,
            BudgetId = originalCashFlow.BudgetId,
            IsSystemGenerated = true,
            AutoApproved = true,
            OriginalCashFlowId = originalCashFlow.Id,
            IsReversal = true,
            IsReversed = false,
            RelatedType = command.SourceType,
            RelatedId = command.SourceId?.ToString()
        };

        // Marquer le CashFlow original comme contre-passe
        originalCashFlow.IsReversed = true;

        // Recuperer le compte pour mettre a jour le solde
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == originalCashFlow.AccountId,
            cancellationToken);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            throw new NotFoundException($"Le compte avec l'ID '{originalCashFlow.AccountId}' n'existe pas");
        }

        // Mettre a jour le solde du compte (inverser l'operation originale)
        if (reversedType == CashFlowType.INCOME)
        {
            // L'original etait EXPENSE, on le contre-passe avec INCOME => on credite
            account.CurrentBalance += reversalCashFlow.Amount;
        }
        else if (reversedType == CashFlowType.EXPENSE)
        {
            // L'original etait INCOME, on le contre-passe avec EXPENSE => on debite
            account.CurrentBalance -= reversalCashFlow.Amount;
        }

        // Mettre a jour le budget si necessaire (pour les EXPENSE qui deviennent INCOME)
        if (originalCashFlow.Type == CashFlowType.EXPENSE && originalCashFlow.BudgetId.HasValue)
        {
            await ReverseBudgetUpdateAsync(originalCashFlow, cancellationToken);
        }

        // Creer les entrees dans l'historique
        var originalHistory = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = originalCashFlow.Id.ToString(),
            Action = CashFlowAction.CANCELLED,
            OldStatus = CashFlowStatus.APPROVED.ToString(),
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = $"Contre-passe par {email}. Motif: {command.Reason}"
        };

        var reversalHistory = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = reversalCashFlow.Id.ToString(),
            Action = CashFlowAction.CREATED,
            OldStatus = null,
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = $"Contre-passation du flux {originalCashFlow.Reference ?? originalCashFlow.Id.ToString()}"
        };

        // Sauvegarder toutes les modifications
        await cashFlowRepository.AddDataAsync(reversalCashFlow, cancellationToken);
        cashFlowRepository.UpdateData(originalCashFlow);
        accountRepository.UpdateData(account);
        await cashFlowHistoryRepository.AddDataAsync(originalHistory, cancellationToken);
        await cashFlowHistoryRepository.AddDataAsync(reversalHistory, cancellationToken);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new ReverseCashFlowResult(
            ReversalCashFlowId: reversalCashFlow.Id,
            OriginalCashFlowId: originalCashFlow.Id,
            Success: true
        );
    }

    /// <summary>
    /// Inverse la mise a jour du budget lors de la contre-passation d'une depense.
    /// Soustrait le montant du SpentAmount du budget.
    /// </summary>
    private async Task ReverseBudgetUpdateAsync(CashFlow originalCashFlow, CancellationToken cancellationToken)
    {
        if (!originalCashFlow.BudgetId.HasValue)
        {
            return;
        }

        var budgets = await budgetRepository.GetByConditionAsync(
            b => b.Id == originalCashFlow.BudgetId.Value,
            cancellationToken);

        var budget = budgets.FirstOrDefault();
        if (budget == null)
        {
            return;
        }

        // Soustraire le montant de la depense annulee
        budget.SpentAmount -= originalCashFlow.Amount;

        // S'assurer que SpentAmount ne devient pas negatif
        if (budget.SpentAmount < 0)
        {
            budget.SpentAmount = 0;
        }

        // Recalculer IsExceeded
        budget.IsExceeded = budget.SpentAmount >= budget.AllocatedAmount;

        budgetRepository.UpdateData(budget);
    }
}
