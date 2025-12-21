using BackendAdmin.Application.Data;
using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceResources;

public class GetServiceResourcesHandler(
    IDockerSwarmService dockerSwarmService,
    IApplicationDbContext dbContext)
    : IQueryHandler<GetServiceResourcesQuery, GetServiceResourcesResult>
{
    public async Task<GetServiceResourcesResult> Handle(
        GetServiceResourcesQuery request,
        CancellationToken cancellationToken)
    {
        // Verifier que le service existe
        var service = await dockerSwarmService.GetServiceByNameAsync(request.ServiceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{request.ServiceName}' non trouve");
        }

        // Recuperer la configuration depuis la base de donnees
        var config = await dbContext.ServiceResourceConfigs
            .FirstOrDefaultAsync(c => c.ServiceName == request.ServiceName, cancellationToken);

        if (config == null)
        {
            // Retourner les ressources actuelles du service Docker
            var dockerResources = GetResourcesFromDockerService(service);
            return new GetServiceResourcesResult(dockerResources);
        }

        // Convertir les valeurs de la base de donnees
        var resourcesDto = new ServiceResourcesDTO(
            ServiceName: config.ServiceName,
            CpuLimit: config.CpuLimit.HasValue ? config.CpuLimit.Value / 1_000_000_000.0 : null,
            CpuReservation: config.CpuReservation.HasValue ? config.CpuReservation.Value / 1_000_000_000.0 : null,
            MemoryLimit: config.MemoryLimit.HasValue ? FormatMemoryBytes(config.MemoryLimit.Value) : null,
            MemoryReservation: config.MemoryReservation.HasValue ? FormatMemoryBytes(config.MemoryReservation.Value) : null,
            PidsLimit: config.PidsLimit,
            BlkioWeight: config.BlkioWeight,
            Ulimits: !string.IsNullOrEmpty(config.UlimitsJson)
                ? JsonSerializer.Deserialize<List<UlimitDTO>>(config.UlimitsJson)
                : null,
            CreatedAt: config.CreatedAt,
            UpdatedAt: config.UpdatedAt
        );

        return new GetServiceResourcesResult(resourcesDto);
    }

    private static ServiceResourcesDTO GetResourcesFromDockerService(Docker.DotNet.Models.SwarmService service)
    {
        var resources = service.Spec.TaskTemplate.Resources;

        double? cpuLimit = null;
        double? cpuReservation = null;
        string? memoryLimit = null;
        string? memoryReservation = null;
        long? pidsLimit = null;

        if (resources?.Limits != null)
        {
            if (resources.Limits.NanoCPUs > 0)
            {
                cpuLimit = resources.Limits.NanoCPUs / 1_000_000_000.0;
            }
            if (resources.Limits.MemoryBytes > 0)
            {
                memoryLimit = FormatMemoryBytes((long)resources.Limits.MemoryBytes);
            }
            if (resources.Limits.Pids > 0)
            {
                pidsLimit = resources.Limits.Pids;
            }
        }

        if (resources?.Reservations != null)
        {
            if (resources.Reservations.NanoCPUs > 0)
            {
                cpuReservation = resources.Reservations.NanoCPUs / 1_000_000_000.0;
            }
            if (resources.Reservations.MemoryBytes > 0)
            {
                memoryReservation = FormatMemoryBytes((long)resources.Reservations.MemoryBytes);
            }
        }

        return new ServiceResourcesDTO(
            ServiceName: service.Spec.Name,
            CpuLimit: cpuLimit,
            CpuReservation: cpuReservation,
            MemoryLimit: memoryLimit,
            MemoryReservation: memoryReservation,
            PidsLimit: pidsLimit,
            BlkioWeight: null,
            Ulimits: null,
            CreatedAt: null,
            UpdatedAt: null
        );
    }

    private static string FormatMemoryBytes(long bytes)
    {
        if (bytes >= 1073741824) // 1 GB
        {
            return $"{bytes / 1073741824.0:0.##}GB";
        }
        if (bytes >= 1048576) // 1 MB
        {
            return $"{bytes / 1048576.0:0.##}MB";
        }
        if (bytes >= 1024) // 1 KB
        {
            return $"{bytes / 1024.0:0.##}KB";
        }
        return $"{bytes}B";
    }
}
