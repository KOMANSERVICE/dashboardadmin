namespace BackendAdmin.Application.Features.Swarm.Commands.PruneImages;

public record PruneImagesCommand(bool Dangling = true) : ICommand<PruneImagesResult>;

public record PruneImagesResult(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedImages
);
