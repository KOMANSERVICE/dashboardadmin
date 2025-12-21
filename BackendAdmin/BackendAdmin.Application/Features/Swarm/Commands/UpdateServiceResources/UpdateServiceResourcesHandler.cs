using BackendAdmin.Application.Data;
using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;
using BackendAdmin.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateServiceResources;

public class UpdateServiceResourcesHandler(
    IDockerSwarmService dockerSwarmService,
    IApplicationDbContext dbContext)
    : ICommandHandler<UpdateServiceResourcesCommand, UpdateServiceResourcesResult>
{
    public async Task<UpdateServiceResourcesResult> Handle(
        UpdateServiceResourcesCommand command,
        CancellationToken cancellationToken)
    {
        // Verifier que le service existe
        var service = await dockerSwarmService.GetServiceByNameAsync(command.ServiceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{command.ServiceName}' non trouve");
        }

        // Convertir les valeurs
        var cpuLimitNano = command.CpuLimit.HasValue
            ? (long)(command.CpuLimit.Value * 1_000_000_000)
            : (long?)null;

        var cpuReservationNano = command.CpuReservation.HasValue
            ? (long)(command.CpuReservation.Value * 1_000_000_000)
            : (long?)null;

        var memoryLimitBytes = !string.IsNullOrEmpty(command.MemoryLimit)
            ? ParseMemoryString(command.MemoryLimit)
            : (long?)null;

        var memoryReservationBytes = !string.IsNullOrEmpty(command.MemoryReservation)
            ? ParseMemoryString(command.MemoryReservation)
            : (long?)null;

        // Sauvegarder la configuration en base
        var existingConfig = await dbContext.ServiceResourceConfigs
            .FirstOrDefaultAsync(c => c.ServiceName == command.ServiceName, cancellationToken);

        if (existingConfig == null)
        {
            existingConfig = new ServiceResourceConfig
            {
                Id = Guid.NewGuid(),
                ServiceName = command.ServiceName,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.ServiceResourceConfigs.Add(existingConfig);
        }

        existingConfig.CpuLimit = cpuLimitNano;
        existingConfig.CpuReservation = cpuReservationNano;
        existingConfig.MemoryLimit = memoryLimitBytes;
        existingConfig.MemoryReservation = memoryReservationBytes;
        existingConfig.PidsLimit = command.PidsLimit;
        existingConfig.BlkioWeight = command.BlkioWeight;
        existingConfig.UlimitsJson = command.Ulimits != null
            ? JsonSerializer.Serialize(command.Ulimits)
            : null;
        existingConfig.UpdatedAt = DateTime.UtcNow;

        await ((DbContext)dbContext).SaveChangesAsync(cancellationToken);

        // Appliquer les ressources au service Docker
        await dockerSwarmService.UpdateServiceResourcesAsync(
            command.ServiceName,
            cpuLimitNano,
            cpuReservationNano,
            memoryLimitBytes,
            memoryReservationBytes,
            command.PidsLimit,
            command.Ulimits,
            cancellationToken);

        return new UpdateServiceResourcesResult(
            command.ServiceName,
            "Ressources mises a jour avec succes"
        );
    }

    private static long ParseMemoryString(string memory)
    {
        memory = memory.Trim().ToUpperInvariant();

        if (long.TryParse(memory, out var bytes))
        {
            return bytes;
        }

        var value = memory[..^2];
        var unit = memory[^2..];

        if (memory.EndsWith("MB") || memory.EndsWith("M"))
        {
            value = memory.EndsWith("MB") ? memory[..^2] : memory[..^1];
            if (double.TryParse(value, out var mb))
            {
                return (long)(mb * 1024 * 1024);
            }
        }
        else if (memory.EndsWith("GB") || memory.EndsWith("G"))
        {
            value = memory.EndsWith("GB") ? memory[..^2] : memory[..^1];
            if (double.TryParse(value, out var gb))
            {
                return (long)(gb * 1024 * 1024 * 1024);
            }
        }
        else if (memory.EndsWith("KB") || memory.EndsWith("K"))
        {
            value = memory.EndsWith("KB") ? memory[..^2] : memory[..^1];
            if (double.TryParse(value, out var kb))
            {
                return (long)(kb * 1024);
            }
        }

        throw new BadRequestException($"Format de memoire invalide: {memory}. Utiliser un format valide (ex: 512MB, 1GB)");
    }
}
