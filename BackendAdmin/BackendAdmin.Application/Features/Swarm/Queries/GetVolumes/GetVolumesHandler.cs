using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetVolumes;

public class GetVolumesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetVolumesQuery, GetVolumesResult>
{
    public async Task<GetVolumesResult> Handle(GetVolumesQuery request, CancellationToken cancellationToken)
    {
        var volumes = await dockerSwarmService.GetVolumesAsync(cancellationToken);

        var volumeDtos = new List<VolumeDTO>();

        foreach (var volume in volumes)
        {
            var containers = await dockerSwarmService.GetContainersUsingVolumeAsync(volume.Name, cancellationToken);
            var size = await dockerSwarmService.GetVolumeSizeAsync(volume.Name, cancellationToken);

            volumeDtos.Add(new VolumeDTO(
                Name: volume.Name,
                Driver: volume.Driver,
                Mountpoint: volume.Mountpoint,
                SizeBytes: size,
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
