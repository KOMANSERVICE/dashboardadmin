using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerChanges;

public class GetContainerChangesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerChangesQuery, GetContainerChangesResult>
{
    public async Task<GetContainerChangesResult> Handle(GetContainerChangesQuery request, CancellationToken cancellationToken)
    {
        var container = await dockerSwarmService.GetContainerByIdAsync(request.ContainerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{request.ContainerId}' non trouve");
        }

        var changes = await dockerSwarmService.GetContainerChangesAsync(request.ContainerId, cancellationToken);

        var containerName = container.Name?.TrimStart('/') ?? request.ContainerId;

        var changeDtos = changes.Select(c => new FilesystemChangeDTO(
            Path: c.Path,
            Kind: GetChangeKind(c.Kind)
        )).ToList();

        return new GetContainerChangesResult(new ContainerChangesDTO(
            ContainerId: request.ContainerId,
            ContainerName: containerName,
            Changes: changeDtos
        ));
    }

    private static string GetChangeKind(Docker.DotNet.Models.FileSystemChangeKind kind)
    {
        // Docker API uses: 0=Modified, 1=Added, 2=Deleted
        return (int)kind switch
        {
            0 => "Modified",
            1 => "Added",
            2 => "Deleted",
            _ => "Unknown"
        };
    }
}
