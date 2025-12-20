namespace BackendAdmin.Application.Features.Swarm.Commands.PruneNetworks;

public record PruneNetworksCommand() : ICommand<PruneNetworksResult>;

public record PruneNetworksResult(int DeletedCount, List<string> DeletedNetworks);
