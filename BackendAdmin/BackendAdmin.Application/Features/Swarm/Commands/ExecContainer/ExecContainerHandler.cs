using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.ExecContainer;

public class ExecContainerHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<ExecContainerCommand, ExecContainerResult>
{
    public async Task<ExecContainerResult> Handle(ExecContainerCommand request, CancellationToken cancellationToken)
    {
        var execRequest = new ContainerExecRequest(
            Command: request.Command,
            Args: request.Args,
            AttachStdout: request.AttachStdout,
            AttachStderr: request.AttachStderr,
            WorkingDir: request.WorkingDir,
            Env: request.Env
        );

        var response = await dockerSwarmService.ExecContainerAsync(
            request.ContainerId,
            execRequest,
            cancellationToken);

        return new ExecContainerResult(response);
    }
}
