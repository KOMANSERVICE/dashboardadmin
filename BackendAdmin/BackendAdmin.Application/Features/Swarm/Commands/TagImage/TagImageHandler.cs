using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.TagImage;

public class TagImageHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<TagImageCommand, TagImageResult>
{
    public async Task<TagImageResult> Handle(TagImageCommand command, CancellationToken cancellationToken)
    {
        var request = new TagImageRequest(
            NewRepository: command.NewRepository,
            NewTag: command.NewTag
        );

        var response = await dockerSwarmService.TagImageAsync(command.ImageId, request, cancellationToken);

        return new TagImageResult(
            SourceImage: response.SourceImage,
            NewRepository: response.NewRepository,
            NewTag: response.NewTag
        );
    }
}
