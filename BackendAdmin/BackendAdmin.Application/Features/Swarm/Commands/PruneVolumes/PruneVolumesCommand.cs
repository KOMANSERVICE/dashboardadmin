using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneVolumes;

public record PruneVolumesCommand() : ICommand<PruneVolumesResult>;

public record PruneVolumesResult(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedVolumes
);
