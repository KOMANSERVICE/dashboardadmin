using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerInspect;

public class GetContainerInspectHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerInspectQuery, GetContainerInspectResult>
{
    public async Task<GetContainerInspectResult> Handle(GetContainerInspectQuery request, CancellationToken cancellationToken)
    {
        var container = await dockerSwarmService.GetContainerByIdAsync(request.ContainerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{request.ContainerId}' non trouve");
        }

        // Get size info
        var sizeInfo = await dockerSwarmService.GetContainerSizeAsync(request.ContainerId, cancellationToken);

        var containerName = container.Name?.TrimStart('/') ?? request.ContainerId;

        // Parse environment variables
        var envVars = new Dictionary<string, string>();
        if (container.Config?.Env != null)
        {
            foreach (var env in container.Config.Env)
            {
                var parts = env.Split('=', 2);
                if (parts.Length == 2)
                {
                    envVars[parts[0]] = parts[1];
                }
            }
        }

        // Parse mounts
        var mounts = container.Mounts?.Select(m => new ContainerMountDTO(
            Type: m.Type,
            Source: m.Source ?? "",
            Destination: m.Destination ?? "",
            ReadOnly: m.RW == false
        )).ToList() ?? new List<ContainerMountDTO>();

        // Parse networks
        var networks = new List<ContainerNetworkDTO>();
        if (container.NetworkSettings?.Networks != null)
        {
            foreach (var network in container.NetworkSettings.Networks)
            {
                networks.Add(new ContainerNetworkDTO(
                    NetworkId: network.Value.NetworkID ?? "",
                    NetworkName: network.Key,
                    IpAddress: network.Value.IPAddress,
                    Gateway: network.Value.Gateway,
                    MacAddress: network.Value.MacAddress
                ));
            }
        }

        // Parse ports
        var ports = new List<ContainerPortDTO>();
        if (container.NetworkSettings?.Ports != null)
        {
            foreach (var port in container.NetworkSettings.Ports)
            {
                var portParts = port.Key.Split('/');
                if (portParts.Length >= 1 && int.TryParse(portParts[0], out var privatePort))
                {
                    var protocol = portParts.Length >= 2 ? portParts[1] : "tcp";

                    if (port.Value != null && port.Value.Any())
                    {
                        foreach (var binding in port.Value)
                        {
                            int.TryParse(binding.HostPort, out var publicPort);
                            ports.Add(new ContainerPortDTO(
                                PrivatePort: privatePort,
                                PublicPort: publicPort,
                                Type: protocol,
                                Ip: binding.HostIP
                            ));
                        }
                    }
                    else
                    {
                        ports.Add(new ContainerPortDTO(
                            PrivatePort: privatePort,
                            PublicPort: null,
                            Type: protocol,
                            Ip: null
                        ));
                    }
                }
            }
        }

        // Parse host config
        var restartPolicyName = container.HostConfig?.RestartPolicy?.Name.ToString() ?? "";
        var hostConfig = new ContainerHostConfigDTO(
            Memory: container.HostConfig?.Memory ?? 0,
            MemorySwap: container.HostConfig?.MemorySwap ?? 0,
            CpuShares: container.HostConfig?.CPUShares ?? 0,
            NanoCpus: container.HostConfig?.NanoCPUs ?? 0,
            RestartPolicy: restartPolicyName,
            Privileged: container.HostConfig?.Privileged ?? false,
            ReadonlyRootfs: container.HostConfig?.ReadonlyRootfs ?? false
        );

        var details = new ContainerDetailsDTO(
            Id: container.ID,
            Name: containerName,
            Image: container.Config?.Image ?? container.Image,
            State: container.State?.Status ?? "unknown",
            Status: GetContainerStatus(container),
            CreatedAt: container.Created,
            Platform: container.Platform ?? "",
            Driver: container.Driver ?? "",
            SizeRootFs: sizeInfo.SizeRootFs,
            SizeRw: sizeInfo.SizeRw,
            Command: container.Config?.Cmd != null ? string.Join(" ", container.Config.Cmd) : null,
            WorkingDir: container.Config?.WorkingDir,
            Labels: container.Config?.Labels?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>(),
            Env: envVars,
            Mounts: mounts,
            Networks: networks,
            Ports: ports,
            HostConfig: hostConfig
        );

        return new GetContainerInspectResult(details);
    }

    private static string GetContainerStatus(Docker.DotNet.Models.ContainerInspectResponse container)
    {
        if (container.State == null) return "unknown";

        if (container.State.Running) return "running";
        if (container.State.Paused) return "paused";
        if (container.State.Restarting) return "restarting";
        if (container.State.Dead) return "dead";

        return container.State.Status ?? "exited";
    }
}
