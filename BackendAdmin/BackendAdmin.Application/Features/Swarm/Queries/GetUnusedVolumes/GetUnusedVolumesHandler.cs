using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetUnusedVolumes;

public class GetUnusedVolumesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetUnusedVolumesQuery, GetUnusedVolumesResult>
{
    public async Task<GetUnusedVolumesResult> Handle(GetUnusedVolumesQuery request, CancellationToken cancellationToken)
    {
        var volumes = await dockerSwarmService.GetUnusedVolumesAsync(cancellationToken);

        var volumeDtos = new List<VolumeDTO>();

        foreach (var volume in volumes)
        {
            var size = await dockerSwarmService.GetVolumeSizeAsync(volume.Name, cancellationToken);

            volumeDtos.Add(new VolumeDTO(
                Name: volume.Name,
                Driver: volume.Driver,
                Mountpoint: volume.Mountpoint,
                SizeBytes: size,
                CreatedAt: ParseCreatedAt(volume.CreatedAt),
                Labels: volume.Labels ?? new Dictionary<string, string>(),
                IsUsed: false
            ));
        }

        return new GetUnusedVolumesResult(volumeDtos);
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
