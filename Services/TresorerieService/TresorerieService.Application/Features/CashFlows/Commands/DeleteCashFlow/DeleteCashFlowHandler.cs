namespace TresorerieService.Application.Features.CashFlows.Commands.DeleteCashFlow;

public class DeleteCashFlowHandler(
    IGenericRepository<CashFlow> cashFlowRepository,
    DbContext dbContext
) : ICommandHandler<DeleteCashFlowCommand, DeleteCashFlowResult>
{
    public async Task<DeleteCashFlowResult> Handle(
        DeleteCashFlowCommand command,
        CancellationToken cancellationToken = default)
    {
        // Recuperer le flux de tresorerie
        var cashFlows = await cashFlowRepository.GetByConditionAsync(
            c => c.Id == command.Id
                 && c.ApplicationId == command.ApplicationId
                 && c.BoutiqueId == command.BoutiqueId,
            cancellationToken);

        var cashFlow = cashFlows.FirstOrDefault();
        if (cashFlow == null)
        {
            throw new NotFoundException("Flux non trouve");
        }

        // Verifier que le flux est en brouillon
        if (cashFlow.Status != CashFlowStatus.DRAFT)
        {
            throw new BadRequestException("Seuls les flux en brouillon peuvent etre supprimes");
        }

        // Verifier que l'utilisateur est l'auteur du flux
        if (cashFlow.CreatedBy != command.UserId)
        {
            throw new BadRequestException("Vous ne pouvez supprimer que vos propres flux");
        }

        // Supprimer le flux de maniere definitive
        dbContext.Set<CashFlow>().Remove(cashFlow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteCashFlowResult(true);
    }
}
