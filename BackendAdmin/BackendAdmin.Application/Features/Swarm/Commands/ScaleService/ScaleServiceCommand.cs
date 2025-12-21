using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.ScaleService;

public record ScaleServiceCommand(string ServiceName, int Replicas) : ICommand<ScaleServiceResult>;

public record ScaleServiceResult(ScaleServiceResponse Response);
