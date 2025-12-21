using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteService;

public class DeleteServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeleteServiceCommand, DeleteServiceResult>
{
    public async Task<DeleteServiceResult> Handle(DeleteServiceCommand command, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DeleteServiceAsync(command.ServiceName, cancellationToken);
        return new DeleteServiceResult(command.ServiceName);
    }
}
