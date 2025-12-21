using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.ScaleService;

public class ScaleServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<ScaleServiceCommand, ScaleServiceResult>
{
    public async Task<ScaleServiceResult> Handle(ScaleServiceCommand request, CancellationToken cancellationToken)
    {
        var (previousReplicas, newReplicas) = await dockerSwarmService.ScaleServiceAsync(
            request.ServiceName,
            request.Replicas,
            cancellationToken);

        var response = new ScaleServiceResponse(
            ServiceName: request.ServiceName,
            PreviousReplicas: previousReplicas,
            NewReplicas: newReplicas
        );

        return new ScaleServiceResult(response);
    }
}
