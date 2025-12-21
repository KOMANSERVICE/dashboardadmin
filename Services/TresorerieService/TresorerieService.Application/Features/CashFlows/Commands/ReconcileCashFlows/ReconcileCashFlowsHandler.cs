namespace TresorerieService.Application.Features.CashFlows.Commands.ReconcileCashFlows;

public class ReconcileCashFlowsHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    IGenericRepository<CashFlowHistory> cashFlowHistoryRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ReconcileCashFlowsCommand, ReconcileCashFlowsResult>
{
    public async Task<ReconcileCashFlowsResult> Handle(
        ReconcileCashFlowsCommand command,
        CancellationToken cancellationToken = default)
    {
        // Verifier que l'utilisateur est manager ou admin
        var userRole = command.UserRole.ToLower();
        if (userRole != "manager" && userRole != "admin")
        {
            throw new BadRequestException("Acces refuse: seul un manager ou admin peut reconcilier des flux");
        }

        // Recuperer tous les flux specifies
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            cf => command.CashFlowIds.Contains(cf.Id)
                  && cf.ApplicationId == command.ApplicationId
                  && cf.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlowList = cashFlows.ToList();

        // Verifier que tous les flux existent
        var foundIds = cashFlowList.Select(cf => cf.Id).ToHashSet();
        var notFoundIds = command.CashFlowIds.Where(id => !foundIds.Contains(id)).ToList();
        if (notFoundIds.Any())
        {
            throw new NotFoundException($"Les flux suivants n'existent pas: {string.Join(", ", notFoundIds)}");
        }

        // Verifier que tous les flux sont APPROVED
        var invalidStatusFlows = cashFlowList.Where(cf => cf.Status != CashFlowStatus.APPROVED).ToList();
        if (invalidStatusFlows.Any())
        {
            var invalidIds = invalidStatusFlows.Select(cf => $"{cf.Id} (Status: {cf.Status})");
            throw new BadRequestException($"Seul un flux approuve peut etre reconcilie. Flux invalides: {string.Join(", ", invalidIds)}");
        }

        // Verifier qu'aucun flux n'est deja reconcilie
        var alreadyReconciledFlows = cashFlowList.Where(cf => cf.IsReconciled).ToList();
        if (alreadyReconciledFlows.Any())
        {
            var reconciledIds = alreadyReconciledFlows.Select(cf => cf.Id.ToString());
            throw new BadRequestException($"Les flux suivants sont deja reconcilies: {string.Join(", ", reconciledIds)}");
        }

        // Reconcilier tous les flux
        var reconciledAt = DateTime.UtcNow;
        var reconciledCashFlows = new List<ReconciledCashFlowDto>();

        foreach (var cashFlow in cashFlowList)
        {
            // Mettre a jour les informations de reconciliation
            cashFlow.IsReconciled = true;
            cashFlow.ReconciledAt = reconciledAt;
            cashFlow.ReconciledBy = command.ReconciledBy;
            cashFlow.BankStatementReference = command.BankStatementReference;
            cashFlow.UpdatedAt = reconciledAt;
            cashFlow.UpdatedBy = command.ReconciledBy;

            // Creer une entree dans l'historique
            var history = new CashFlowHistory
            {
                Id = Guid.NewGuid(),
                CashFlowId = cashFlow.Id.ToString(),
                Action = CashFlowAction.RECONCILED,
                OldStatus = cashFlow.Status.ToString(),
                NewStatus = cashFlow.Status.ToString(),
                Comment = string.IsNullOrEmpty(command.BankStatementReference)
                    ? $"Flux reconcilie en masse par {command.ReconciledBy}"
                    : $"Flux reconcilie en masse par {command.ReconciledBy} - Ref: {command.BankStatementReference}",
                CreatedAt = reconciledAt,
                CreatedBy = command.ReconciledBy,
                UpdatedAt = reconciledAt,
                UpdatedBy = command.ReconciledBy
            };

            await cashFlowHistoryRepository.AddDataAsync(history, cancellationToken);
            cashFlowRepository.UpdateData(cashFlow);

            reconciledCashFlows.Add(new ReconciledCashFlowDto(
                Id: cashFlow.Id,
                Reference: cashFlow.Reference,
                Label: cashFlow.Label,
                Amount: cashFlow.Amount,
                ReconciledAt: reconciledAt,
                BankStatementReference: command.BankStatementReference
            ));
        }

        // Sauvegarder toutes les modifications en une seule transaction
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new ReconcileCashFlowsResult(
            ReconciledCount: reconciledCashFlows.Count,
            ReconciledCashFlows: reconciledCashFlows
        );
    }
}
