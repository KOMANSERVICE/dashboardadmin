namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteImage;

public record DeleteImageCommand(
    string ImageId,
    bool Force = false,
    bool PruneChildren = false
) : ICommand<DeleteImageResult>;

public record DeleteImageResult(bool Success);
