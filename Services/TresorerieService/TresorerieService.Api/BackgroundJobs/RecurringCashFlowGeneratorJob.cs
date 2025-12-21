using IDR.Library.BuildingBlocks.CQRS;
using TresorerieService.Application.Features.RecurringCashFlows.Jobs;

namespace TresorerieService.Api.BackgroundJobs;

/// <summary>
/// Job planifie qui s'execute chaque jour a minuit pour generer automatiquement
/// les flux de tresorerie a partir des flux recurrents.
/// </summary>
public class RecurringCashFlowGeneratorJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringCashFlowGeneratorJob> _logger;
    private readonly TimeSpan _executionTime;

    public RecurringCashFlowGeneratorJob(
        IServiceProvider serviceProvider,
        ILogger<RecurringCashFlowGeneratorJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Execution a minuit UTC
        _executionTime = TimeSpan.Zero;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[RecurringCashFlowGeneratorJob] Service demarre. Execution prevue chaque jour a minuit UTC.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = CalculateNextRunTime(now);
            var delay = nextRun - now;

            _logger.LogInformation(
                "[RecurringCashFlowGeneratorJob] Prochaine execution prevue le {NextRun} (dans {Delay})",
                nextRun.ToString("yyyy-MM-dd HH:mm:ss"),
                delay.ToString(@"hh\:mm\:ss"));

            try
            {
                // Attendre jusqu'a la prochaine execution
                await Task.Delay(delay, stoppingToken);

                // Executer le job
                await ExecuteJobAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[RecurringCashFlowGeneratorJob] Service arrete par annulation.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[RecurringCashFlowGeneratorJob] Erreur lors de l'execution du job: {Message}",
                    ex.Message);

                // Attendre 5 minutes avant de reessayer en cas d'erreur
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Calcule le prochain moment d'execution (minuit UTC du jour suivant si deja passe).
    /// </summary>
    private DateTime CalculateNextRunTime(DateTime now)
    {
        var today = now.Date;
        var nextRun = today.Add(_executionTime);

        // Si l'heure d'execution est passee pour aujourd'hui, planifier pour demain
        if (now >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }

    /// <summary>
    /// Execute la generation des flux recurrents.
    /// </summary>
    private async Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[RecurringCashFlowGeneratorJob] Debut de l'execution du job a {DateTime}",
            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        using var scope = _serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new GenerateRecurringCashFlowsCommand();
        var result = await sender.Send(command, stoppingToken);

        _logger.LogInformation(
            "[RecurringCashFlowGeneratorJob] Job termine: {Generated} flux generes, {Approved} approuves, {Pending} en attente, {Errors} erreurs",
            result.GeneratedCount,
            result.ApprovedCount,
            result.PendingCount,
            result.ErrorCount);
    }
}
