using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PushImage;

public class PushImageHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PushImageCommand, PushImageResult>
{
    public async Task<PushImageResult> Handle(PushImageCommand command, CancellationToken cancellationToken)
    {
        var request = new PushImageRequest(
            Tag: command.Tag,
            Registry: command.Registry
        );

        var response = await dockerSwarmService.PushImageAsync(command.ImageId, request, cancellationToken);

        return new PushImageResult(
            ImageName: response.ImageName,
            Tag: response.Tag,
            Status: response.Status,
            PushedAt: response.PushedAt
        );
    }
}
