using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.RollbackService;

public class RollbackServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<RollbackServiceCommand, RollbackServiceResult>
{
    public async Task<RollbackServiceResult> Handle(RollbackServiceCommand command, CancellationToken cancellationToken)
    {
        await dockerSwarmService.RollbackServiceAsync(command.ServiceName, cancellationToken);
        return new RollbackServiceResult(command.ServiceName);
    }
}
