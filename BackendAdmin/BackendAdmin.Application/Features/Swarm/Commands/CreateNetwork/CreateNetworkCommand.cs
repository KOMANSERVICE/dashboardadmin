namespace BackendAdmin.Application.Features.Swarm.Commands.CreateNetwork;

public record CreateNetworkCommand(
    string Name,
    string Driver = "overlay",
    bool IsAttachable = true,
    string? Subnet = null,
    string? Gateway = null,
    string? IpRange = null,
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? Options = null
) : ICommand<CreateNetworkResult>;

public record CreateNetworkResult(string NetworkId, string NetworkName);
