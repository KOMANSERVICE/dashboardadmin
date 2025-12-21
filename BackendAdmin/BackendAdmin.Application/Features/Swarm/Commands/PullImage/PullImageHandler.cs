using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PullImage;

public class PullImageHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PullImageCommand, PullImageResult>
{
    public async Task<PullImageResult> Handle(PullImageCommand command, CancellationToken cancellationToken)
    {
        var request = new PullImageRequest(
            Image: command.Image,
            Tag: command.Tag,
            Registry: command.Registry
        );

        var response = await dockerSwarmService.PullImageAsync(request, cancellationToken);

        return new PullImageResult(
            ImageName: response.ImageName,
            Tag: response.Tag,
            Status: response.Status,
            PulledAt: response.PulledAt
        );
    }
}
