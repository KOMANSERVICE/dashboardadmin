using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateService;

public class UpdateServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<UpdateServiceCommand, UpdateServiceResult>
{
    public async Task<UpdateServiceResult> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        var request = new UpdateServiceRequest(
            Image: command.Image,
            Replicas: command.Replicas,
            Env: command.Env,
            Labels: command.Labels
        );

        await dockerSwarmService.UpdateServiceAsync(command.ServiceName, request, cancellationToken);

        return new UpdateServiceResult(command.ServiceName);
    }
}
