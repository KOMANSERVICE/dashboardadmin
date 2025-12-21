using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.CreateService;

public class CreateServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<CreateServiceCommand, CreateServiceResult>
{
    public async Task<CreateServiceResult> Handle(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        var request = new CreateServiceRequest(
            Name: command.Name,
            Image: command.Image,
            Replicas: command.Replicas,
            Ports: command.Ports,
            Env: command.Env,
            Labels: command.Labels
        );

        var serviceId = await dockerSwarmService.CreateServiceAsync(request, cancellationToken);

        return new CreateServiceResult(serviceId, command.Name);
    }
}
