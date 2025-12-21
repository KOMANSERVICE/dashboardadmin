using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteStack;

public class DeleteStackHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeleteStackCommand, DeleteStackResult>
{
    public async Task<DeleteStackResult> Handle(DeleteStackCommand command, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DeleteStackAsync(command.StackName, cancellationToken);

        return new DeleteStackResult(command.StackName);
    }
}
