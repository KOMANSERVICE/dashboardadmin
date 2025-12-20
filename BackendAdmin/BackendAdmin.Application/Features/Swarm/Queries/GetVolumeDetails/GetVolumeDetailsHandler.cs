using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetVolumeDetails;

public class GetVolumeDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetVolumeDetailsQuery, GetVolumeDetailsResult>
{
    public async Task<GetVolumeDetailsResult> Handle(GetVolumeDetailsQuery request, CancellationToken cancellationToken)
    {
        var volume = await dockerSwarmService.GetVolumeByNameAsync(request.Name, cancellationToken);

        if (volume == null)
        {
            throw new NotFoundException($"Volume '{request.Name}' non trouve");
        }

        var containers = await dockerSwarmService.GetContainersUsingVolumeAsync(volume.Name, cancellationToken);
        var size = await dockerSwarmService.GetVolumeSizeAsync(volume.Name, cancellationToken);

        var volumeDetails = new VolumeDetailsDTO(
            Name: volume.Name,
            Driver: volume.Driver,
            Mountpoint: volume.Mountpoint,
            SizeBytes: size,
            CreatedAt: ParseCreatedAt(volume.CreatedAt),
            Labels: volume.Labels ?? new Dictionary<string, string>(),
            IsUsed: containers.Count > 0,
            Options: volume.Options ?? new Dictionary<string, string>(),
            Scope: volume.Scope ?? "local",
            UsedByContainers: containers.ToList()
        );

        return new GetVolumeDetailsResult(volumeDetails);
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
