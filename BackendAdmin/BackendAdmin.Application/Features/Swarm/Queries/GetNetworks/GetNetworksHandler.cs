using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNetworks;

public class GetNetworksHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetNetworksQuery, GetNetworksResult>
{
    public async Task<GetNetworksResult> Handle(GetNetworksQuery request, CancellationToken cancellationToken)
    {
        var networks = await dockerSwarmService.GetNetworksAsync(cancellationToken);

        var networkDtos = new List<NetworkDTO>();

        foreach (var network in networks)
        {
            var containerCount = network.Containers?.Count ?? 0;

            networkDtos.Add(new NetworkDTO(
                Id: network.ID,
                Name: network.Name,
                Driver: network.Driver,
                Scope: network.Scope,
                IsInternal: network.Internal,
                IsAttachable: network.Attachable,
                CreatedAt: ParseCreatedAt(network.Created),
                ContainerCount: containerCount
            ));
        }

        return new GetNetworksResult(networkDtos);
    }

    private static DateTime ParseCreatedAt(DateTime? created)
    {
        return created ?? DateTime.UtcNow;
    }
}
