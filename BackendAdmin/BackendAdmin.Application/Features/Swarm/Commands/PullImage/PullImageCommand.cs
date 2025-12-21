using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.PullImage;

public record PullImageCommand(
    string Image,
    string? Tag = "latest",
    string? Registry = null
) : ICommand<PullImageResult>;

public record PullImageResult(
    string ImageName,
    string Tag,
    string Status,
    DateTime PulledAt
);
