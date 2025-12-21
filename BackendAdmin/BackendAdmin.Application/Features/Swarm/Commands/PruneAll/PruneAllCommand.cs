using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneAll;

public record PruneAllCommand() : ICommand<PruneAllResult>;

public record PruneAllResult(PruneAllResponseDTO Response);
