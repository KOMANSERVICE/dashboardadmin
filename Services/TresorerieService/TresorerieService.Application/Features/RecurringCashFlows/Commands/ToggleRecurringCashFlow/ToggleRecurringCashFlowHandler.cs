namespace TresorerieService.Application.Features.RecurringCashFlows.Commands.ToggleRecurringCashFlow;

public class ToggleRecurringCashFlowHandler(
    IGenericRepository<RecurringCashFlow> recurringCashFlowRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ToggleRecurringCashFlowCommand, ToggleRecurringCashFlowResult>
{
    public async Task<ToggleRecurringCashFlowResult> Handle(
        ToggleRecurringCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le flux recurrent avec filtres de securite
        var recurringCashFlows = await recurringCashFlowRepository.GetByConditionAsync(
            r => r.Id == command.Id
                 && r.ApplicationId == command.ApplicationId
                 && r.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var recurringCashFlow = recurringCashFlows.FirstOrDefault();
        if (recurringCashFlow == null)
        {
            throw new NotFoundException("Flux recurrent non trouve");
        }

        // Toggle IsActive
        recurringCashFlow.IsActive = !recurringCashFlow.IsActive;

        // Mettre a jour les champs d'audit
        recurringCashFlow.UpdatedAt = DateTime.UtcNow;
        recurringCashFlow.UpdatedBy = command.UserId;

        recurringCashFlowRepository.UpdateData(recurringCashFlow);
        await unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new ToggleRecurringCashFlowResult(
            Id: recurringCashFlow.Id,
            IsActive: recurringCashFlow.IsActive
        );
    }
}
