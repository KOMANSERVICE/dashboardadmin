using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImageDetails;

public class GetImageDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetImageDetailsQuery, GetImageDetailsResult>
{
    public async Task<GetImageDetailsResult> Handle(GetImageDetailsQuery request, CancellationToken cancellationToken)
    {
        var image = await dockerSwarmService.GetImageByIdAsync(request.ImageId, cancellationToken);

        if (image == null)
        {
            throw new NotFoundException($"Image '{request.ImageId}' non trouvee");
        }

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

        // Build config DTO
        var configDto = new ImageConfigDTO(
            Hostname: image.Config?.Hostname,
            Domainname: image.Config?.Domainname,
            User: image.Config?.User,
            Cmd: image.Config?.Cmd?.ToList(),
            Entrypoint: image.Config?.Entrypoint?.ToList(),
            Env: image.Config?.Env?.ToList(),
            WorkingDir: image.Config?.WorkingDir,
            ExposedPorts: image.Config?.ExposedPorts?.Keys.ToList(),
            Volumes: image.Config?.Volumes?.Keys.ToList()
        );

        var imageDto = new ImageDetailsDTO(
            Id: image.ID.Replace("sha256:", ""),
            Repository: repository,
            Tag: tag,
            Size: image.Size,
            CreatedAt: image.Created,
            Author: image.Author,
            Architecture: image.Architecture,
            Os: image.Os,
            DockerVersion: image.DockerVersion,
            Labels: image.Config?.Labels ?? new Dictionary<string, string>(),
            RepoTags: image.RepoTags?.ToList() ?? new List<string>(),
            RepoDigests: image.RepoDigests?.ToList() ?? new List<string>(),
            Comment: image.Comment,
            Container: image.Container,
            Config: configDto
        );

        return new GetImageDetailsResult(imageDto);
    }
}
