using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetVolumes;

public class GetVolumesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetVolumesQuery, GetVolumesResult>
{
    public async Task<GetVolumesResult> Handle(GetVolumesQuery request, CancellationToken cancellationToken)
    {
        // Use batch method to get all volumes info in a single operation for better performance
        // This avoids N+1 queries (one call per volume for containers and size)
        var volumesInfo = await dockerSwarmService.GetAllVolumesInfoAsync(cancellationToken);
        var volumes = await dockerSwarmService.GetVolumesAsync(cancellationToken);

        var volumeDtos = new List<VolumeDTO>();

        foreach (var volume in volumes)
        {
            // Get volume info from batch result
            var (containers, sizeBytes) = volumesInfo.GetValueOrDefault(volume.Name, (new List<string>(), 0L));

            volumeDtos.Add(new VolumeDTO(
                Name: volume.Name,
                Driver: volume.Driver,
                Mountpoint: volume.Mountpoint,
                SizeBytes: sizeBytes,
                CreatedAt: ParseCreatedAt(volume.CreatedAt),
                Labels: volume.Labels ?? new Dictionary<string, string>(),
                IsUsed: containers.Count > 0
            ));
        }

        return new GetVolumesResult(volumeDtos);
    }

    private static DateTime ParseCreatedAt(string createdAt)
    {
        if (DateTime.TryParse(createdAt, out var result))
        {
            return result;
        }
        return DateTime.UtcNow;
    }
}
