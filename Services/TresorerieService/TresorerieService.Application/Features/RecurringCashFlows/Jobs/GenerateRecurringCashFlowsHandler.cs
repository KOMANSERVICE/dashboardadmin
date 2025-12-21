using Microsoft.Extensions.Logging;

namespace TresorerieService.Application.Features.RecurringCashFlows.Jobs;

/// <summary>
/// Handler pour generer automatiquement les flux de tresorerie a partir des flux recurrents.
/// Execute quotidiennement a minuit pour creer les CashFlow dus.
/// </summary>
public class GenerateRecurringCashFlowsHandler(
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository,
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IGenericRepository<Account> accountRepository,
    IUnitOfWork unitOfWork,
    ILogger<GenerateRecurringCashFlowsHandler> logger
) : ICommandHandler<GenerateRecurringCashFlowsCommand, GenerateRecurringCashFlowsResult>
{
    public async Task<GenerateRecurringCashFlowsResult> Handle(
        GenerateRecurringCashFlowsCommand command,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var generatedIds = new List<Guid>();
        var approvedCount = 0;
        var pendingCount = 0;
        var errorCount = 0;

        logger.LogInformation(
            "[RecurringCashFlowJob] Demarrage de la generation des flux recurrents pour le {Date}",
            today.ToString("yyyy-MM-dd"));

        // Recuperer tous les flux recurrents actifs avec NextOccurrence <= aujourd'hui
        // et EndDate null ou >= aujourd'hui
        var recurringCashFlows = await recurringCashFlowRepository.GetByConditionAsync(
            rcf => rcf.IsActive
                   && rcf.NextOccurrence.Date <= today
                   && (rcf.EndDate == null || rcf.EndDate.Value.Date >= today),
            cancellationToken);

        logger.LogInformation(
            "[RecurringCashFlowJob] {Count} flux recurrents a traiter",
            recurringCashFlows.Count);

        foreach (var recurringCashFlow in recurringCashFlows)
        {
            try
            {
                var result = await ProcessRecurringCashFlowAsync(
                    recurringCashFlow,
                    cancellationToken);

                generatedIds.Add(result.CashFlowId);
                if (result.IsApproved)
                    approvedCount++;
                else
                    pendingCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                logger.LogError(
                    ex,
                    "[RecurringCashFlowJob] Erreur lors du traitement du flux recurrent {RecurringCashFlowId}: {Message}",
                    recurringCashFlow.Id,
                    ex.Message);
                // Continuer avec les autres flux meme si un echoue
            }
        }

        logger.LogInformation(
            "[RecurringCashFlowJob] Generation terminee: {Generated} generes, {Approved} approuves, {Pending} en attente, {Errors} erreurs",
            generatedIds.Count,
            approvedCount,
            pendingCount,
            errorCount);

        return new GenerateRecurringCashFlowsResult(
            GeneratedCount: generatedIds.Count,
            ApprovedCount: approvedCount,
            PendingCount: pendingCount,
            ErrorCount: errorCount,
            GeneratedCashFlowIds: generatedIds);
    }

    /// <summary>
    /// Traite un flux recurrent: cree le CashFlow et met a jour NextOccurrence.
    /// Retourne le resultat du traitement.
    /// </summary>
    private async Task<ProcessResult> ProcessRecurringCashFlowAsync(
        RecurringCashFlow recurringCashFlow,
        CancellationToken cancellationToken)
    {
        // Creer le nouveau CashFlow
        var cashFlow = new CashFlow
        {
            Id = Guid.NewGuid(),
            ApplicationId = recurringCashFlow.ApplicationId,
            BoutiqueId = recurringCashFlow.BoutiqueId,
            Reference = GenerateReference(recurringCashFlow),
            Type = recurringCashFlow.Type,
            CategoryId = recurringCashFlow.CategoryId,
            Label = recurringCashFlow.Label,
            Description = recurringCashFlow.Description,
            Amount = recurringCashFlow.Amount,
            TaxAmount = 0,
            TaxRate = 0,
            Currency = "XOF",
            AccountId = recurringCashFlow.AccountId,
            DestinationAccountId = null,
            PaymentMethod = recurringCashFlow.PaymentMethod,
            Date = recurringCashFlow.NextOccurrence,
            ThirdPartyType = null,
            ThirdPartyName = recurringCashFlow.ThirdPartyName,
            ThirdPartyId = null,
            // Workflow - depend de AutoValidate
            Status = recurringCashFlow.AutoValidate ? CashFlowStatus.APPROVED : CashFlowStatus.PENDING,
            SubmittedAt = DateTime.UtcNow,
            SubmittedBy = "SYSTEM",
            ValidatedAt = recurringCashFlow.AutoValidate ? DateTime.UtcNow : null,
            ValidatedBy = recurringCashFlow.AutoValidate ? "SYSTEM" : null,
            RejectionReason = null,
            // Reconciliation
            IsReconciled = false,
            ReconciledAt = null,
            ReconciledBy = null,
            BankStatementReference = null,
            // Recurrence
            IsRecurring = true,
            RecurringCashFlowId = recurringCashFlow.Id.ToString(),
            // Systeme
            IsSystemGenerated = true,
            AutoApproved = recurringCashFlow.AutoValidate,
            AttachmentUrl = null,
            RelatedType = null,
            RelatedId = null,
            // Audit
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            UpdatedBy = "SYSTEM"
        };

        await cashFlowRepository.AddDataAsync(cashFlow, cancellationToken);

        logger.LogInformation(
            "[RecurringCashFlowJob] CashFlow {CashFlowId} cree a partir du flux recurrent {RecurringCashFlowId} (Status: {Status})",
            cashFlow.Id,
            recurringCashFlow.Id,
            cashFlow.Status);

        var isApproved = recurringCashFlow.AutoValidate;

        // Si AutoValidate = true, mettre a jour le solde du compte
        if (isApproved)
        {
            var accounts = await accountRepository.GetByConditionAsync(
                a => a.Id == recurringCashFlow.AccountId,
                cancellationToken);
            var account = accounts.FirstOrDefault();

            if (account != null)
            {
                // Mettre a jour le solde selon le type de flux
                if (recurringCashFlow.Type == CashFlowType.INCOME)
                {
                    account.CurrentBalance += recurringCashFlow.Amount;
                }
                else if (recurringCashFlow.Type == CashFlowType.EXPENSE)
                {
                    account.CurrentBalance -= recurringCashFlow.Amount;
                }

                account.UpdatedAt = DateTime.UtcNow;
                account.UpdatedBy = "SYSTEM";
                accountRepository.UpdateData(account);

                logger.LogInformation(
                    "[RecurringCashFlowJob] Solde du compte {AccountId} mis a jour: {NewBalance} ({Type} de {Amount})",
                    account.Id,
                    account.CurrentBalance,
                    recurringCashFlow.Type,
                    recurringCashFlow.Amount);
            }

            // Creer l'entree d'historique pour l'approbation
            var history = new CashFlowHistory
            {
                Id = Guid.NewGuid(),
                CashFlowId = cashFlow.Id.ToString(),
                Action = CashFlowAction.APPROVED,
                OldStatus = CashFlowStatus.PENDING.ToString(),
                NewStatus = CashFlowStatus.APPROVED.ToString(),
                Comment = "Flux auto-approuve (generation automatique)",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "SYSTEM"
            };

            await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        }
        else
        {
            // Creer l'entree d'historique pour la creation
            var history = new CashFlowHistory
            {
                Id = Guid.NewGuid(),
                CashFlowId = cashFlow.Id.ToString(),
                Action = CashFlowAction.SUBMITTED,
                OldStatus = null,
                NewStatus = CashFlowStatus.PENDING.ToString(),
                Comment = "Flux genere automatiquement, en attente de validation",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "SYSTEM"
            };

            await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
        }

        // Mettre a jour le flux recurrent
        recurringCashFlow.LastGeneratedAt = DateTime.UtcNow;
        recurringCashFlow.NextOccurrence = CalculateNextOccurrence(
            recurringCashFlow.NextOccurrence,
            recurringCashFlow.Frequency,
            recurringCashFlow.Interval,
            recurringCashFlow.DayOfMonth,
            recurringCashFlow.DayOfWeek);
        recurringCashFlow.UpdatedAt = DateTime.UtcNow;
        recurringCashFlow.UpdatedBy = "SYSTEM";

        recurringCashFlowRepository.UpdateData(recurringCashFlow);

        logger.LogInformation(
            "[RecurringCashFlowJob] Flux recurrent {RecurringCashFlowId} mis a jour, prochaine occurrence: {NextOccurrence}",
            recurringCashFlow.Id,
            recurringCashFlow.NextOccurrence.ToString("yyyy-MM-dd"));

        // Sauvegarder toutes les modifications pour ce flux
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new ProcessResult(cashFlow.Id, isApproved);
    }

    /// <summary>
    /// Resultat du traitement d'un flux recurrent.
    /// </summary>
    private record ProcessResult(Guid CashFlowId, bool IsApproved);

    /// <summary>
    /// Genere une reference unique pour le CashFlow.
    /// Format: REC-{YYYYMMDD}-{Random4}
    /// </summary>
    private static string GenerateReference(RecurringCashFlow recurringCashFlow)
    {
        var dateStr = recurringCashFlow.NextOccurrence.ToString("yyyyMMdd");
        var random = Guid.NewGuid().ToString("N")[..4].ToUpper();
        return $"REC-{dateStr}-{random}";
    }

    /// <summary>
    /// Calcule la prochaine occurrence basee sur la frequence et les parametres.
    /// </summary>
    private static DateTime CalculateNextOccurrence(
        DateTime currentOccurrence,
        RecurringFrequency frequency,
        int interval,
        int? dayOfMonth,
        int? dayOfWeek)
    {
        var nextDate = currentOccurrence;

        switch (frequency)
        {
            case RecurringFrequency.DAILY:
                nextDate = nextDate.AddDays(interval);
                break;

            case RecurringFrequency.WEEKLY:
                // Pour WEEKLY, on ajoute simplement N semaines (7 * interval jours)
                // Le jour de la semaine est deja correct car l'occurrence actuelle est sur le bon jour
                nextDate = nextDate.AddDays(7 * interval);
                break;

            case RecurringFrequency.MONTHLY:
                nextDate = nextDate.AddMonths(interval);
                // Ajuster au jour du mois si specifie
                if (dayOfMonth.HasValue)
                {
                    var targetDay = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                    nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay,
                        nextDate.Hour, nextDate.Minute, nextDate.Second, DateTimeKind.Utc);
                }
                break;

            case RecurringFrequency.QUARTERLY:
                nextDate = nextDate.AddMonths(3 * interval);
                // Ajuster au jour du mois si specifie
                if (dayOfMonth.HasValue)
                {
                    var targetDay = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                    nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay,
                        nextDate.Hour, nextDate.Minute, nextDate.Second, DateTimeKind.Utc);
                }
                break;

            case RecurringFrequency.YEARLY:
                nextDate = nextDate.AddYears(interval);
                // Ajuster au jour du mois si specifie (gere le 29 fevrier)
                if (dayOfMonth.HasValue)
                {
                    var targetDay = Math.Min(dayOfMonth.Value, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                    nextDate = new DateTime(nextDate.Year, nextDate.Month, targetDay,
                        nextDate.Hour, nextDate.Minute, nextDate.Second, DateTimeKind.Utc);
                }
                break;
        }

        return nextDate;
    }
}
