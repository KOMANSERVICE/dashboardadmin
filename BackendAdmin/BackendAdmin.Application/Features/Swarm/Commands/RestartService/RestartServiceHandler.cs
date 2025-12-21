using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.RestartService;

public class RestartServiceHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<RestartServiceCommand, RestartServiceResult>
{
    public async Task<RestartServiceResult> Handle(RestartServiceCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.RestartServiceAsync(request.ServiceName, cancellationToken);
        return new RestartServiceResult("Service redemarre avec succes");
    }
}
