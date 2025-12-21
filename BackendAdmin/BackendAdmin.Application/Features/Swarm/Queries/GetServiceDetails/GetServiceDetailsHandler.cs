using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceDetails;

public class GetServiceDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetServiceDetailsQuery, GetServiceDetailsResult>
{
    public async Task<GetServiceDetailsResult> Handle(GetServiceDetailsQuery request, CancellationToken cancellationToken)
    {
        var service = await dockerSwarmService.GetServiceByNameAsync(request.ServiceName, cancellationToken);

        if (service == null)
        {
            throw new NotFoundException($"Service '{request.ServiceName}' non trouve");
        }

        // Parse ports
        var ports = service.Endpoint?.Ports?.Select(p => new ServicePortDTO(
            TargetPort: (int)p.TargetPort,
            PublishedPort: (int)p.PublishedPort,
            Protocol: p.Protocol ?? "tcp"
        )).ToList() ?? new List<ServicePortDTO>();

        // Parse environment variables
        var env = new Dictionary<string, string>();
        if (service.Spec.TaskTemplate.ContainerSpec?.Env != null)
        {
            foreach (var envVar in service.Spec.TaskTemplate.ContainerSpec.Env)
            {
                var parts = envVar.Split('=', 2);
                if (parts.Length == 2)
                {
                    env[parts[0]] = parts[1];
                }
            }
        }

        // Parse labels
        var labels = service.Spec.Labels != null
            ? new Dictionary<string, string>(service.Spec.Labels)
            : new Dictionary<string, string>();

        // Parse networks
        var networks = service.Spec.TaskTemplate.Networks?.Select(n => n.Target ?? "unknown").ToList() ?? new List<string>();

        var serviceDto = new ServiceDetailsDTO(
            Id: service.ID,
            Name: service.Spec.Name,
            Replicas: GetRunningReplicas(service),
            DesiredReplicas: (int)(service.Spec.Mode.Replicated?.Replicas ?? 0UL),
            Image: service.Spec.TaskTemplate.ContainerSpec?.Image ?? "unknown",
            Status: GetServiceStatus(service),
            CreatedAt: service.CreatedAt,
            UpdatedAt: service.UpdatedAt,
            Ports: ports,
            Env: env,
            Labels: labels,
            Networks: networks
        );

        return new GetServiceDetailsResult(serviceDto);
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
