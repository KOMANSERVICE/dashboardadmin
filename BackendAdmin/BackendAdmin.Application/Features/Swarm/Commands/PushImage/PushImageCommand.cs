namespace BackendAdmin.Application.Features.Swarm.Commands.PushImage;

public record PushImageCommand(
    string ImageId,
    string? Tag = null,
    string? Registry = null
) : ICommand<PushImageResult>;

public record PushImageResult(
    string ImageName,
    string Tag,
    string Status,
    DateTime PushedAt
);
