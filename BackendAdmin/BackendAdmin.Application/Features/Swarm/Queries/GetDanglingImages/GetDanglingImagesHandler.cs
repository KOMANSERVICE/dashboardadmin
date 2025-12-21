using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDanglingImages;

public class GetDanglingImagesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetDanglingImagesQuery, GetDanglingImagesResult>
{
    public async Task<GetDanglingImagesResult> Handle(GetDanglingImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await dockerSwarmService.GetDanglingImagesAsync(cancellationToken);

        var imageDtos = new List<ImageDTO>();

        foreach (var image in images)
        {
            var containerCount = await dockerSwarmService.GetImageContainerCountAsync(image.ID, cancellationToken);

            imageDtos.Add(new ImageDTO(
                Id: image.ID.Replace("sha256:", ""),
                Repository: null,
                Tag: null,
                Size: image.Size,
                CreatedAt: image.Created,
                ContainerCount: containerCount,
                IsDangling: true
            ));
        }

        return new GetDanglingImagesResult(imageDtos);
    }
}
