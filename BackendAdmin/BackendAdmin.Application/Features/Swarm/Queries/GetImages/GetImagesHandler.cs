using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImages;

public class GetImagesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetImagesQuery, GetImagesResult>
{
    public async Task<GetImagesResult> Handle(GetImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await dockerSwarmService.GetImagesAsync(request.All, cancellationToken);

        var imageDtos = new List<ImageDTO>();

        foreach (var image in images)
        {
            var containerCount = await dockerSwarmService.GetImageContainerCountAsync(image.ID, cancellationToken);

            // Parse repository and tag from RepoTags
            var repoTag = image.RepoTags?.FirstOrDefault();
            string? repository = null;
            string? tag = null;

            if (!string.IsNullOrEmpty(repoTag) && repoTag != "<none>:<none>")
            {
                var parts = repoTag.Split(':');
                repository = parts[0] != "<none>" ? parts[0] : null;
                tag = parts.Length > 1 && parts[1] != "<none>" ? parts[1] : null;
            }

            var isDangling = string.IsNullOrEmpty(repository) && string.IsNullOrEmpty(tag);

            imageDtos.Add(new ImageDTO(
                Id: image.ID.Replace("sha256:", ""),
                Repository: repository,
                Tag: tag,
                Size: image.Size,
                CreatedAt: image.Created,
                ContainerCount: containerCount,
                IsDangling: isDangling
            ));
        }

        return new GetImagesResult(imageDtos);
    }
}
