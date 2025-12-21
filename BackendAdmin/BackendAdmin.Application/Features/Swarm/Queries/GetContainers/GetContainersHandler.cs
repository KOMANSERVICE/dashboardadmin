using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainers;

public class GetContainersHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainersQuery, GetContainersResult>
{
    public async Task<GetContainersResult> Handle(GetContainersQuery request, CancellationToken cancellationToken)
    {
        var containers = await dockerSwarmService.GetContainersAsync(request.All, cancellationToken);

        var containerDtos = containers.Select(c => new ContainerDTO(
            Id: c.ID,
            Name: c.Names?.FirstOrDefault()?.TrimStart('/') ?? c.ID,
            Image: c.Image,
            State: c.State,
            Status: c.Status,
            CreatedAt: c.Created
        )).ToList();

        return new GetContainersResult(containerDtos);
    }
}
