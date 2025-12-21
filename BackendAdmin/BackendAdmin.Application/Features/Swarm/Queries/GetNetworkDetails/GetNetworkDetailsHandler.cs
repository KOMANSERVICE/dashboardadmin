using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNetworkDetails;

public class GetNetworkDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetNetworkDetailsQuery, GetNetworkDetailsResult>
{
    public async Task<GetNetworkDetailsResult> Handle(GetNetworkDetailsQuery request, CancellationToken cancellationToken)
    {
        var network = await dockerSwarmService.GetNetworkByNameAsync(request.NetworkName, cancellationToken);

        if (network == null)
        {
            throw new NotFoundException($"Reseau '{request.NetworkName}' non trouve");
        }

        // Extract IPAM configuration
        string? subnet = null;
        string? gateway = null;
        string? ipRange = null;

        if (network.IPAM?.Config != null && network.IPAM.Config.Count > 0)
        {
            var ipamConfig = network.IPAM.Config[0];
            subnet = ipamConfig.Subnet;
            gateway = ipamConfig.Gateway;
            ipRange = ipamConfig.IPRange;
        }

        // Map containers
        var containers = new List<NetworkContainerDTO>();
        if (network.Containers != null)
        {
            foreach (var (containerId, containerInfo) in network.Containers)
            {
                containers.Add(new NetworkContainerDTO(
                    ContainerId: containerId,
                    ContainerName: containerInfo.Name,
                    IpAddress: containerInfo.IPv4Address,
                    MacAddress: containerInfo.MacAddress
                ));
            }
        }

        var networkDetails = new NetworkDetailsDTO(
            Id: network.ID,
            Name: network.Name,
            Driver: network.Driver,
            Scope: network.Scope,
            IsInternal: network.Internal,
            IsAttachable: network.Attachable,
            CreatedAt: network.Created, //?? DateTime.UtcNow, // a corrigé
            Subnet: subnet,
            Gateway: gateway,
            IpRange: ipRange,
            Labels: network.Labels ?? new Dictionary<string, string>(),
            Options: network.Options ?? new Dictionary<string, string>(),
            Containers: containers
        );

        return new GetNetworkDetailsResult(networkDetails);
    }
}
