using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetSwarmServices;

public class GetSwarmServicesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetSwarmServicesQuery, GetSwarmServicesResult>
{
    public async Task<GetSwarmServicesResult> Handle(GetSwarmServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await dockerSwarmService.GetServicesAsync(cancellationToken);

        var serviceDtos = services.Select(s => new SwarmServiceDTO(
            Id: s.ID,
            Name: s.Spec.Name,
            Replicas: GetRunningReplicas(s),
            DesiredReplicas: (int)(s.Spec.Mode.Replicated?.Replicas ?? 0UL),
            Image: s.Spec.TaskTemplate.ContainerSpec.Image,
            Status: GetServiceStatus(s),
            CreatedAt: s.CreatedAt,
            UpdatedAt: s.UpdatedAt
        )).ToList();

        return new GetSwarmServicesResult(serviceDtos);
    }

    private static int GetRunningReplicas(Docker.DotNet.Models.SwarmService service)
    {
        if (service.ServiceStatus == null)
            return 0;

        return (int)service.ServiceStatus.RunningTasks;
    }

    private static string GetServiceStatus(Docker.DotNet.Models.SwarmService service)
    {
        if (service.ServiceStatus == null)
            return "unknown";

        var running = service.ServiceStatus.RunningTasks;
        var desired = service.Spec.Mode.Replicated?.Replicas ?? 0UL;

        if (running == desired && running > 0)
            return "running";
        if (running == 0 && desired == 0)
            return "stopped";
        if (running < desired)
            return "starting";

        return "unknown";
    }
}
