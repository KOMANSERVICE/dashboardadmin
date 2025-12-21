using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.TagImage;

public record TagImageCommand(
    string ImageId,
    string NewRepository,
    string NewTag
) : ICommand<TagImageResult>;

public record TagImageResult(
    string SourceImage,
    string NewRepository,
    string NewTag
);
