namespace TresorerieService.Application.Features.RecurringCashFlows.Jobs;

/// <summary>
/// Command pour generer automatiquement les flux de tresorerie a partir des flux recurrents.
/// Cette command est appelee par le job CRON quotidien.
/// </summary>
public record GenerateRecurringCashFlowsCommand() : ICommand<GenerateRecurringCashFlowsResult>;

/// <summary>
/// Resultat de la generation des flux recurrents.
/// </summary>
/// <param name="GeneratedCount">Nombre de flux generes</param>
/// <param name="ApprovedCount">Nombre de flux auto-approuves</param>
/// <param name="PendingCount">Nombre de flux en attente</param>
/// <param name="ErrorCount">Nombre d'erreurs rencontrees</param>
/// <param name="GeneratedCashFlowIds">IDs des flux generes</param>
public record GenerateRecurringCashFlowsResult(
    int GeneratedCount,
    int ApprovedCount,
    int PendingCount,
    int ErrorCount,
    List<Guid> GeneratedCashFlowIds
);
