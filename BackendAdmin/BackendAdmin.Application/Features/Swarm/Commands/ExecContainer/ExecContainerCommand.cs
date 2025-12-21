using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.ExecContainer;

public record ExecContainerCommand(
    string ContainerId,
    string Command,
    string[]? Args = null,
    bool AttachStdout = true,
    bool AttachStderr = true,
    string? WorkingDir = null,
    Dictionary<string, string>? Env = null
) : ICommand<ExecContainerResult>;

public record ExecContainerResult(ContainerExecResponse Response);
