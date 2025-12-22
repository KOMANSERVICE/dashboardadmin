using IDR.Library.BuildingBlocks.Contexts.Services;
using TresorerieService.Application.Features.CashFlows.DTOs;

namespace TresorerieService.Application.Features.CashFlows.Commands.ApproveCashFlow;

public class ApproveCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Account> accountRepository,
    IGenericRepository<Budget> budgetRepository,
    IGenericRepository<BudgetAlert> budgetAlertRepository,
    IUnitOfWork unitOfWork,
    IUserContextService userContextService
) : ICommandHandler<ApproveCashFlowCommand, ApproveCashFlowResult>
{
    public async Task<ApproveCashFlowResult> Handle(
        ApproveCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verifier que l'utilisateur est manager ou admin
        //TODO: pas bien gerer je vais moi meme m'en occuper plus tard
        //var userRole = command.UserRole.ToLower();
        //if (userRole != "manager" && userRole != "admin")
        //{
        //    throw new BadRequestException("Acces refuse: seul un manager ou admin peut valider un flux");
        //}

        var email = userContextService.GetEmail();

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

        // Verifier que le flux est en PENDING
        if (cashFlow.Status != CashFlowStatus.PENDING)
        {
            throw new BadRequestException($"Seul un flux en attente (PENDING) peut etre valide. Statut actuel: {cashFlow.Status}");
        }

        // Verifier que le flux n'est pas un TRANSFER (deja approuve automatiquement)
        if (cashFlow.Type == CashFlowType.TRANSFER)
        {
            throw new BadRequestException("Les transferts sont approuves automatiquement et ne passent pas par ce workflow");
        }

        // Recuperer le compte associe
        var accounts = await accountRepository.GetByConditionAsync(
            a => a.Id == cashFlow.AccountId,
            cancellationToken);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            throw new NotFoundException($"Le compte avec l'ID '{cashFlow.AccountId}' n'existe pas");
        }

        // Stocker l'ancien statut pour l'historique
        var oldStatus = cashFlow.Status.ToString();

        // Mettre a jour le statut et les informations de validation
        cashFlow.Status = CashFlowStatus.APPROVED;
        cashFlow.ValidatedAt = DateTime.UtcNow;
        cashFlow.ValidatedBy = email;

        // Mettre a jour le solde du compte selon le type de flux
        if (cashFlow.Type == CashFlowType.INCOME)
        {
            // Crediter le compte pour un revenu
            account.CurrentBalance += cashFlow.Amount;
        }
        else if (cashFlow.Type == CashFlowType.EXPENSE)
        {
            // Debiter le compte pour une depense
            account.CurrentBalance -= cashFlow.Amount;

            // Mettre a jour le budget si un budget est associe
            await UpdateBudgetForExpenseAsync(cashFlow, cancellationToken);
        }

        // Creer une entree dans l'historique
        var history = new CashFlowHistory
        {
            Id = Guid.NewGuid(),
            CashFlowId = cashFlow.Id.ToString(),
            Action = CashFlowAction.APPROVED,
            OldStatus = oldStatus,
            NewStatus = CashFlowStatus.APPROVED.ToString(),
            Comment = "Flux valide par " + email
        };

        // Sauvegarder les modifications
        await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        cashFlowRepository.UpdateData(cashFlow);
        accountRepository.UpdateData(account);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        // Recuperer les informations supplementaires pour le DTO
        var categories = await categoryRepository.GetByConditionAsync(
            c => c.Id.ToString() == cashFlow.CategoryId,
            cancellationToken);
        var category = categories.FirstOrDefault();

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
            AccountName: account.Name,
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

        return new ApproveCashFlowResult(cashFlowDto, account.CurrentBalance);
    }

    /// <summary>
    /// Met a jour le budget associe a une depense approuvee.
    /// - Ajoute le montant au spentAmount du budget
    /// - Met a jour le flag isExceeded si spentAmount >= allocatedAmount
    /// - Cree une alerte si le seuil est atteint ou depasse
    /// </summary>
    private async Task UpdateBudgetForExpenseAsync(CashFlow cashFlow, CancellationToken cancellationToken)
    {
        // Verifier si le flux a un budget associe
        if (!cashFlow.BudgetId.HasValue)
        {
            return;
        }

        // Recuperer le budget
        var budgets = await budgetRepository.GetByConditionAsync(
            b => b.Id == cashFlow.BudgetId.Value && b.IsActive,
            cancellationToken);

        var budget = budgets.FirstOrDefault();
        if (budget == null)
        {
            // Budget non trouve ou inactif, on ne fait rien
            return;
        }

        // Stocker les anciennes valeurs pour detecter les changements
        var wasExceeded = budget.IsExceeded;
        var previousSpentAmount = budget.SpentAmount;

        // Ajouter le montant de la depense au montant depense
        budget.SpentAmount += cashFlow.Amount;

        // Verifier si le budget est depasse
        if (budget.SpentAmount >= budget.AllocatedAmount)
        {
            budget.IsExceeded = true;
        }

        // Mettre a jour le budget
        budgetRepository.UpdateData(budget);

        // Calculer le taux de consommation
        var consumptionRate = budget.AllocatedAmount > 0
            ? (budget.SpentAmount / budget.AllocatedAmount) * 100
            : 0;

        // Creer une alerte si necessaire
        await CreateBudgetAlertIfNeededAsync(
            budget,
            cashFlow,
            wasExceeded,
            previousSpentAmount,
            consumptionRate,
            cancellationToken);
    }

    /// <summary>
    /// Cree une alerte budgetaire si le seuil est atteint ou si le budget est depasse.
    /// </summary>
    private async Task CreateBudgetAlertIfNeededAsync(
        Budget budget,
        CashFlow cashFlow,
        bool wasExceeded,
        decimal previousSpentAmount,
        decimal consumptionRate,
        CancellationToken cancellationToken)
    {
        string? alertType = null;
        string? message = null;

        // Cas 1: Budget vient d'etre depasse (transition de non-depasse a depasse)
        if (budget.IsExceeded && !wasExceeded)
        {
            alertType = "EXCEEDED";
            message = $"Le budget '{budget.Name}' a ete depasse. " +
                      $"Montant alloue: {budget.AllocatedAmount} {budget.Currency}, " +
                      $"Montant depense: {budget.SpentAmount} {budget.Currency}";
        }
        // Cas 2: Seuil d'alerte vient d'etre atteint (sans depasser le budget)
        else if (!budget.IsExceeded)
        {
            var previousConsumptionRate = budget.AllocatedAmount > 0
                ? (previousSpentAmount / budget.AllocatedAmount) * 100
                : 0;

            // Verifier si on vient de franchir le seuil d'alerte
            if (consumptionRate >= budget.AlertThreshold && previousConsumptionRate < budget.AlertThreshold)
            {
                alertType = "THRESHOLD_REACHED";
                message = $"Le budget '{budget.Name}' a atteint {consumptionRate:F1}% de consommation. " +
                          $"Seuil d'alerte: {budget.AlertThreshold}%. " +
                          $"Montant restant: {budget.RemainingAmount} {budget.Currency}";
            }
        }

        // Creer l'alerte si necessaire
        if (alertType != null)
        {
            var alert = new BudgetAlert
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                CashFlowId = cashFlow.Id,
                AlertType = alertType,
                SpentAmountAtAlert = budget.SpentAmount,
                AllocatedAmountAtAlert = budget.AllocatedAmount,
                ConsumptionRate = consumptionRate,
                ThresholdAtAlert = budget.AlertThreshold,
                Message = message,
                IsAcknowledged = false,
                CreatedBy = cashFlow.ValidatedBy ?? "system"
            };

            await budgetAlertRepository.AddDataAsync(alert, cancellationToken);
        }
    }
}
